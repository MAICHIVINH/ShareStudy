using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CustomValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (authResponse, refreshToken) = await _authService.RegisterAsync(request, cancellationToken);
        SetRefreshTokenCookie(refreshToken);

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Đăng ký tài khoản thành công", 201));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CustomValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (authResponse, refreshToken) = await _authService.LoginAsync(request, cancellationToken);
        SetRefreshTokenCookie(refreshToken);

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Đăng nhập thành công"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest? request, CancellationToken cancellationToken)
    {
        var token = request?.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedException("Refresh Token không tồn tại trong Cookie hoặc Body.");
        }

        var (authResponse, newRefreshToken) = await _authService.RefreshTokenAsync(token, cancellationToken);
        SetRefreshTokenCookie(newRefreshToken);

        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Làm mới Token thành công"));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken)
    {
        var token = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            await _authService.LogoutAsync(token, cancellationToken);
        }

        Response.Cookies.Delete("refreshToken");

        return Ok(ApiResponse.SuccessResponse("Đăng xuất thành công"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var user = await _authService.GetCurrentUserAsync(_currentUserService.UserId.Value, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Lấy thông tin tài khoản thành công"));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Path = "/api/auth"
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
