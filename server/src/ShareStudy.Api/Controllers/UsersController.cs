using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Application.DTOs.User;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("Vui lòng đăng nhập.");
        }

        var updatedUser = await _userService.UpdateProfileAsync(_currentUserService.UserId.Value, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(updatedUser, "Cập nhật thông tin thành công"));
    }

    [HttpPost("avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("Vui lòng đăng nhập.");
        }

        var avatarUrl = await _userService.UploadAvatarAsync(_currentUserService.UserId.Value, file, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { avatarUrl }, "Tải ảnh đại diện lên MinIO thành công"));
    }
}
