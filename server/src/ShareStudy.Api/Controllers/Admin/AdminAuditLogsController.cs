using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Policy = "AdminOnly")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAdminManagementService _adminService;

    public AdminAuditLogsController(IAdminManagementService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] string? keyword,
        [FromQuery] string? module,
        [FromQuery] string? severity,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetAuditLogsAsync(keyword, module, severity, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.SuccessResponse(result));
    }
}
