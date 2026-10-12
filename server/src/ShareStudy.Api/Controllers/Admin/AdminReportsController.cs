using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Report;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Policy = "ModeratorOrAdmin")]
public class AdminReportsController : ControllerBase
{
    private readonly IAdminManagementService _adminService;
    private readonly ICurrentUserService _currentUserService;

    public AdminReportsController(IAdminManagementService adminService, ICurrentUserService currentUserService)
    {
        _adminService = adminService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ReportDto>>>> GetReports(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetReportsAsync(status, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReportDto>>.SuccessResponse(result));
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<ReportDto>>> ResolveReport(
        long id,
        [FromBody] ResolveReportRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId ?? 0;
        var result = await _adminService.ResolveReportAsync(id, adminId, request, cancellationToken);
        return Ok(ApiResponse<ReportDto>.SuccessResponse(result, "Xử lý báo cáo vi phạm thành công"));
    }
}
