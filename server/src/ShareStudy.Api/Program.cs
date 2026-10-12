using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShareStudy.Api.Middlewares;
using ShareStudy.Api.Services;
using ShareStudy.Application;
using ShareStudy.Application.Interfaces;
using ShareStudy.Infrastructure;
using ShareStudy.Infrastructure.Persistence;
using ShareStudy.Infrastructure.Persistence.Seed;

// 0. Automatically load environment variables from .env file (if present)
LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

// Render dynamic PORT binding (if PORT env var is present in Render cloud)
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// 1. Add Application and Infrastructure Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
    options.ValueLengthLimit = 52428800;
});

// 2. Configure CORS Policy (Supports Local Dev + Vercel Production)
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var defaultOrigins = new[] { 
    "http://localhost:5173", 
    "https://localhost:5173", 
    "https://sharestudy.vercel.app" 
};
var allowedOrigins = (configuredOrigins != null && configuredOrigins.Length > 0) ? configuredOrigins : defaultOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Configure JWT Bearer Authentication & Authorization Policies
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    secretKey = builder.Configuration["JWT_SECRET"] ?? "ShareStudySecretKeyForJwtAuthenticationTokensMustBeAtLeast32BytesLong!";
}
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

var issuer = jwtSettings["Issuer"];
if (string.IsNullOrWhiteSpace(issuer))
{
    issuer = builder.Configuration["JWT_ISSUER"] ?? "ShareStudyApi";
}

var audience = jwtSettings["Audience"];
if (string.IsNullOrWhiteSpace(audience))
{
    audience = builder.Configuration["JWT_AUDIENCE"] ?? "ShareStudyClient";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Admin", "Moderator"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configure Swagger with JWT Authorize Button
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShareStudy API",
        Version = "v1",
        Description = "API cho Nền tảng Chia sẻ Tài liệu Học tập ShareStudy"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Token JWT với định dạng: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 5. Auto Migrate and Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// 6. HTTP Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable Swagger in Development & Production for API Testing
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShareStudy API v1"));

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MaintenanceModeMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

app.MapControllers();

app.Run();

static void LoadEnvFile()
{
    var current = Directory.GetCurrentDirectory();
    while (!string.IsNullOrEmpty(current))
    {
        var envPath = Path.Combine(current, ".env");
        if (File.Exists(envPath))
        {
            foreach (var rawLine in File.ReadAllLines(envPath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0) continue;

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                if ((value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith('\'') && value.EndsWith('\'')))
                {
                    value = value[1..^1];
                }

                if (Environment.GetEnvironmentVariable(key) == null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }

                // Map standard shorthand to ASP.NET Core hierarchical naming
                if (key.Equals("DefaultConnection", StringComparison.OrdinalIgnoreCase) &&
                    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") == null)
                {
                    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", value);
                }
                else if (key.Equals("JWT_SECRET", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Jwt__Secret") == null)
                {
                    Environment.SetEnvironmentVariable("Jwt__Secret", value);
                }
                else if (key.Equals("JWT_ISSUER", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Jwt__Issuer") == null)
                {
                    Environment.SetEnvironmentVariable("Jwt__Issuer", value);
                }
                else if (key.Equals("JWT_AUDIENCE", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Jwt__Audience") == null)
                {
                    Environment.SetEnvironmentVariable("Jwt__Audience", value);
                }
                else if (key.Equals("JWT_EXPIRY_MINUTES", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Jwt__ExpiryMinutes") == null)
                {
                    Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", value);
                }
                else if (key.Equals("MINIO_ENDPOINT", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Minio__Endpoint") == null)
                {
                    Environment.SetEnvironmentVariable("Minio__Endpoint", value);
                }
                else if (key.Equals("MINIO_ACCESS_KEY", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Minio__AccessKey") == null)
                {
                    Environment.SetEnvironmentVariable("Minio__AccessKey", value);
                }
                else if (key.Equals("MINIO_SECRET_KEY", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Minio__SecretKey") == null)
                {
                    Environment.SetEnvironmentVariable("Minio__SecretKey", value);
                }
                else if (key.Equals("MINIO_BUCKET_NAME", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Minio__BucketName") == null)
                {
                    Environment.SetEnvironmentVariable("Minio__BucketName", value);
                }
                else if (key.Equals("MINIO_USE_SSL", StringComparison.OrdinalIgnoreCase) &&
                         Environment.GetEnvironmentVariable("Minio__UseSSL") == null)
                {
                    Environment.SetEnvironmentVariable("Minio__UseSSL", value);
                }
            }
            break;
        }

        var parent = Directory.GetParent(current);
        if (parent == null || parent.FullName == current) break;
        current = parent.FullName;
    }
}
