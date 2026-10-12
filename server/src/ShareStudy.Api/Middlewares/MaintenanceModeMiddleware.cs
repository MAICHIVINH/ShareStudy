using System.Text.Json;
using ShareStudy.Application.Common;
using ShareStudy.Application.Interfaces;
using ShareStudy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShareStudy.Api.Middlewares;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Exempt admin endpoints, auth endpoints, and public settings
        if (path.StartsWith("/api/admin") || 
            path.StartsWith("/api/auth") || 
            path.StartsWith("/api/settings/public") || 
            path.StartsWith("/swagger") ||
            path.StartsWith("/uploads"))
        {
            await _next(context);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<IAdminManagementService>();
        var settings = await adminService.GetSystemSettingsAsync(context.RequestAborted);

        if (settings.MaintenanceMode)
        {
            // Check if user is logged in as Admin
            var user = context.User;
            var isAdmin = user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");

            if (!isAdmin)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<string>.FailureResponse(
                    "Hệ thống đang trong quá trình bảo trì định kỳ để nâng cấp hạ tầng. Vui lòng quay lại sau!",
                    503
                );

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}
