using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/settings")]
[AllowAnonymous]
public class PublicSettingsController : ControllerBase
{
    private readonly IAdminManagementService _adminService;

    public PublicSettingsController(IAdminManagementService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> GetPublicSettings(CancellationToken cancellationToken)
    {
        var settings = await _adminService.GetSystemSettingsAsync(cancellationToken);
        
        // Hide SMTP sensitive details for public callers
        settings.SmtpEmail = string.Empty;
        settings.SmtpHost = string.Empty;

        return Ok(ApiResponse<SystemSettingsDto>.SuccessResponse(settings));
    }
}
