using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "ModeratorOrAdmin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminManagementService _adminService;

    public AdminDashboardController(IAdminManagementService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result));
    }

    [HttpGet("charts")]
    public async Task<ActionResult<ApiResponse<DashboardChartsDto>>> GetCharts(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardChartsAsync(cancellationToken);
        return Ok(ApiResponse<DashboardChartsDto>.SuccessResponse(result));
    }
}
