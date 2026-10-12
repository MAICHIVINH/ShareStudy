using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Middlewares;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Only log mutating requests or auth requests
        bool shouldLog = method != "GET" || path.Contains("/login") || path.Contains("/download");

        if (!shouldLog || path.StartsWith("/swagger") || path.StartsWith("/uploads"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var ipAddress = GetClientIpAddress(context);
        
        await _next(context);
        
        sw.Stop();

        // Perform logging asynchronously after response
        try
        {
            var user = context.User;
            var userName = user.Identity?.IsAuthenticated == true
                ? (user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("username")?.Value ?? "User")
                : "Khách (Guest)";

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? userId = long.TryParse(userIdClaim, out var id) ? id : null;

            var (module, actionType, actionName) = ResolveActionDetails(method, path);
            int statusCode = context.Response.StatusCode;

            string severity = statusCode switch
            {
                >= 200 and < 300 => "SUCCESS",
                >= 400 and < 500 => "WARNING",
                >= 500 => "ERROR",
                _ => "INFO"
            };

            string details = $"Thực thi [{method}] {path} - Trạng thái: {statusCode} ({sw.ElapsedMilliseconds}ms)";

            var payloadObj = new
            {
                path = context.Request.Path.Value,
                method = method,
                queryString = context.Request.QueryString.Value,
                statusCode = statusCode,
                durationMs = sw.ElapsedMilliseconds
            };

            using var scope = serviceProvider.CreateScope();
            var adminService = scope.ServiceProvider.GetRequiredService<IAdminManagementService>();
            
            await adminService.LogActivityAsync(
                performer: userName,
                ipAddress: ipAddress,
                actionType: actionType,
                actionName: actionName,
                module: module,
                severity: severity,
                details: details,
                payload: payloadObj
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tự động ghi audit log");
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // 1. Cloudflare IP
        var cfIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cfIp)) return cfIp.Trim();

        // 2. X-Real-IP Header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp)) return realIp.Trim();

        // 3. X-Forwarded-For Header (Take the first client IP before proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(firstIp)) return firstIp;
        }

        // 4. RemoteIpAddress fallback
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Normalize IPv6 loopback ::1 to 127.0.0.1
            if (System.Net.IPAddress.IsLoopback(remoteIp))
            {
                return "127.0.0.1";
            }

            // Normalize IPv4-mapped IPv6 address (e.g. ::ffff:192.168.1.5) to clean IPv4 string
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }

            return remoteIp.ToString();
        }

        return "127.0.0.1";
    }

    private static (string Module, string ActionType, string ActionName) ResolveActionDetails(string method, string path)
    {
        if (path.Contains("/auth/login")) return ("Auth", "LOGIN", "Đăng nhập hệ thống");
        if (path.Contains("/auth/register")) return ("Auth", "REGISTER", "Đăng ký tài khoản mới");
        if (path.Contains("/auth/refresh-token")) return ("Auth", "REFRESH_TOKEN", "Làm mới Token phiên");
        
        if (path.Contains("/admin/users") && path.Contains("status")) return ("Users", "TOGGLE_USER_STATUS", "Bật/Tắt trạng thái người dùng");
        if (path.Contains("/admin/users") && path.Contains("roles")) return ("Users", "UPDATE_ROLE", "Cập nhật vai trò người dùng");
        if (path.Contains("/admin/users") && path.Contains("reset-password")) return ("Users", "RESET_PASSWORD", "Reset mật khẩu người dùng");
        if (path.Contains("/admin/users") && method == "DELETE") return ("Users", "DELETE_USER", "Xóa/Khóa tài khoản người dùng");
        if (path.Contains("/admin/users")) return ("Users", "MANAGE_USERS", "Quản lý tài khoản người dùng");

        if (path.Contains("/admin/documents") && path.Contains("approve")) return ("Documents", "APPROVE_DOCUMENT", "Phê duyệt tài liệu");
        if (path.Contains("/admin/documents") && path.Contains("reject")) return ("Documents", "REJECT_DOCUMENT", "Từ chối tài liệu");
        if (path.Contains("/admin/documents") && path.Contains("hide")) return ("Documents", "HIDE_DOCUMENT", "Ẩn/Hiện tài liệu");
        if (path.Contains("/admin/documents") && method == "DELETE") return ("Documents", "HARD_DELETE_DOCUMENT", "Xóa vĩnh viễn tài liệu");

        if (path.Contains("/documents") && method == "POST") return ("Documents", "UPLOAD_DOCUMENT", "Tải lên tài liệu mới");
        if (path.Contains("/documents") && path.Contains("download")) return ("Documents", "DOWNLOAD_DOCUMENT", "Tải xuống tài liệu");
        if (path.Contains("/documents") && method == "DELETE") return ("Documents", "DELETE_DOCUMENT", "Xóa tài liệu");

        if (path.Contains("/admin/reports") && path.Contains("resolve")) return ("Reports", "RESOLVE_REPORT", "Xử lý báo cáo vi phạm");
        if (path.Contains("/reports") && method == "POST") return ("Reports", "CREATE_REPORT", "Gửi báo cáo vi phạm");

        if (path.Contains("/admin/schools")) return ("Education", method == "DELETE" ? "DELETE_SCHOOL" : "SAVE_SCHOOL", "Cập nhật danh mục Trường");
        if (path.Contains("/admin/faculties")) return ("Education", method == "DELETE" ? "DELETE_FACULTY" : "SAVE_FACULTY", "Cập nhật danh mục Khoa");
        if (path.Contains("/admin/majors")) return ("Education", method == "DELETE" ? "DELETE_MAJOR" : "SAVE_MAJOR", "Cập nhật danh mục Ngành");
        if (path.Contains("/admin/subjects")) return ("Education", method == "DELETE" ? "DELETE_SUBJECT" : "SAVE_SUBJECT", "Cập nhật danh mục Môn học");

        if (path.Contains("/admin/settings")) return ("Settings", "UPDATE_SETTINGS", "Cập nhật cài đặt hệ thống");

        string module = path.Contains("/admin") ? "System" : "Interaction";
        string actionType = method switch
        {
            "POST" => "CREATE",
            "PUT" => "UPDATE",
            "PATCH" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "ACTION"
        };
        string actionName = $"Thao tác [{method}] {path}";

        return (module, actionType, actionName);
    }
}
