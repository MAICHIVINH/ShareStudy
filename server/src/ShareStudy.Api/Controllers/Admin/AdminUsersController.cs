using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminManagementService _adminService;

    public AdminUsersController(IAdminManagementService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserDto>>>> GetUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] string? role,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersAsync(searchTerm, status, role, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminUserDto>>.SuccessResponse(result));
    }

    [HttpPut("{id}/toggle-status")]
    [HttpPatch("{id}/toggle-status")]
    [HttpPut("{id}/status")]
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> ToggleUserStatus(long id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ToggleUserStatusAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.SuccessResponse(result, "Cập nhật trạng thái tài khoản thành công"));
    }

    [HttpPut("{id}/role")]
    [HttpPatch("{id}/role")]
    [HttpPut("{id}/roles")]
    [HttpPatch("{id}/roles")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateUserRole(
        long id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateUserRoleAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminUserDto>.SuccessResponse(result, "Cập nhật quyền vai trò thành công"));
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetUserPassword(long id, CancellationToken cancellationToken)
    {
        var tempPassword = await _adminService.ResetUserPasswordAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.SuccessResponse(tempPassword, "Reset mật khẩu thành công. Mật khẩu mới: " + tempPassword));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteUserAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xóa tài khoản người dùng thành công"));
    }
}
