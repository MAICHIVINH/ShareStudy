using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.DTOs.Document;
using ShareStudy.Application.DTOs.Report;

namespace ShareStudy.Application.Interfaces;

public interface IAdminManagementService
{
    // Admin Documents
    Task ApproveDocumentAsync(long documentId, CancellationToken cancellationToken = default);
    Task RejectDocumentAsync(long documentId, RejectDocumentRequest request, CancellationToken cancellationToken = default);
    Task HideDocumentAsync(long documentId, CancellationToken cancellationToken = default);
    Task HardDeleteDocumentAsync(long documentId, CancellationToken cancellationToken = default);

    // Admin Users
    Task<PagedResult<AdminUserDto>> GetUsersAsync(string? searchTerm, string? status, string? role, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<AdminUserDto> ToggleUserStatusAsync(long userId, CancellationToken cancellationToken = default);
    Task<AdminUserDto> UpdateUserRoleAsync(long userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<string> ResetUserPasswordAsync(long userId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(long userId, CancellationToken cancellationToken = default);

    // Admin Reports
    Task<PagedResult<ReportDto>> GetReportsAsync(string? status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ReportDto> ResolveReportAsync(long reportId, long adminId, ResolveReportRequest request, CancellationToken cancellationToken = default);

    // Admin Dashboard Analytics
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<DashboardChartsDto> GetDashboardChartsAsync(CancellationToken cancellationToken = default);

    // Admin Audit Logs & System Settings
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? keyword, string? module, string? severity, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task LogActivityAsync(string performer, string ipAddress, string actionType, string actionName, string module, string severity, string details, object? payload = null, CancellationToken cancellationToken = default);
    Task<SystemSettingsDto> GetSystemSettingsAsync(CancellationToken cancellationToken = default);
    Task<SystemSettingsDto> UpdateSystemSettingsAsync(SystemSettingsDto request, CancellationToken cancellationToken = default);
}
