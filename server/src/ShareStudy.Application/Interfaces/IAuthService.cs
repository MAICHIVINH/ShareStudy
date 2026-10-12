using ShareStudy.Application.DTOs.Auth;

namespace ShareStudy.Application.Interfaces;

public interface IAuthService
{
    Task<(AuthResponse AuthResponse, string RefreshToken)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<(AuthResponse AuthResponse, string RefreshToken)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<(AuthResponse AuthResponse, string RefreshToken)> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task LogoutAsync(string token, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default);
}
