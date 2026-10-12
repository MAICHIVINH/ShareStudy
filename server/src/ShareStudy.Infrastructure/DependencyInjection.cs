using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Infrastructure.Persistence;
using ShareStudy.Infrastructure.Repositories;
using ShareStudy.Infrastructure.Security;
using ShareStudy.Infrastructure.Storage;

namespace ShareStudy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration["ConnectionStrings:DefaultConnection"] 
                ?? configuration["DefaultConnection"];
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        var r2AccessKey = configuration["CloudflareR2:AccessKeyId"] ?? configuration["R2:AccessKeyId"];
        var isR2Configured = !string.IsNullOrWhiteSpace(r2AccessKey) 
                             || string.Equals(configuration["Storage:Provider"], "R2", StringComparison.OrdinalIgnoreCase);

        if (isR2Configured)
        {
            var accountId = configuration["CloudflareR2:AccountId"] ?? configuration["R2:AccountId"] ?? "";
            var accessKey = r2AccessKey ?? "";
            var secretKey = configuration["CloudflareR2:SecretAccessKey"] ?? configuration["R2:SecretAccessKey"] ?? "";
            var serviceUrl = configuration["CloudflareR2:ServiceUrl"] 
                             ?? configuration["R2:ServiceUrl"] 
                             ?? $"https://{accountId}.r2.cloudflarestorage.com";

            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            services.AddSingleton<IAmazonS3>(new AmazonS3Client(accessKey, secretKey, s3Config));
            services.AddScoped<IFileStorageService, R2FileStorageService>();
        }
        else
        {
            var minioEndpoint = configuration["Minio:Endpoint"];
            if (string.IsNullOrWhiteSpace(minioEndpoint)) minioEndpoint = configuration["MINIO_ENDPOINT"] ?? "localhost:9000";

            var minioAccessKey = configuration["Minio:AccessKey"];
            if (string.IsNullOrWhiteSpace(minioAccessKey)) minioAccessKey = configuration["MINIO_ACCESS_KEY"] ?? "minioadmin";

            var minioSecretKey = configuration["Minio:SecretKey"];
            if (string.IsNullOrWhiteSpace(minioSecretKey)) minioSecretKey = configuration["MINIO_SECRET_KEY"] ?? "minioadmin";

            var minioUseSsl = configuration.GetValue<bool>("Minio:UseSSL", false);
            if (!minioUseSsl && bool.TryParse(configuration["MINIO_USE_SSL"], out var parsedSsl))
            {
                minioUseSsl = parsedSsl;
            }

            services.AddMinio(configureClient => configureClient
                .WithEndpoint(minioEndpoint)
                .WithCredentials(minioAccessKey, minioSecretKey)
                .WithSSL(minioUseSsl)
                .Build());

            services.AddScoped<IFileStorageService, MinioFileStorageService>();
        }

        return services;
    }
}
