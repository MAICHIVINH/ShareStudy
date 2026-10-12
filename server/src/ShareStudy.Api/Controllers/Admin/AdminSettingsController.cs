using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = "AdminOnly")]
public class AdminSettingsController : ControllerBase
{
    private readonly IAdminManagementService _adminService;

    public AdminSettingsController(IAdminManagementService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetSystemSettingsAsync(cancellationToken);
        return Ok(ApiResponse<SystemSettingsDto>.SuccessResponse(result));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> UpdateSettings(
        [FromBody] SystemSettingsDto request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateSystemSettingsAsync(request, cancellationToken);
        return Ok(ApiResponse<SystemSettingsDto>.SuccessResponse(result, "Cập nhật cài đặt hệ thống thành công"));
    }
}
