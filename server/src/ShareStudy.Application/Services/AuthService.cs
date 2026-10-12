using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Application.Interfaces;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;
using ShareStudy.Application.Interfaces.Repositories;

namespace ShareStudy.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<(AuthResponse AuthResponse, string RefreshToken)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();

        var settingRepo = _unitOfWork.Repository<SystemSetting>();
        var allowRegSetting = await settingRepo.Query().FirstOrDefaultAsync(s => s.Key == "AllowRegistration", cancellationToken);
        if (allowRegSetting != null && bool.TryParse(allowRegSetting.Value, out var allowReg) && !allowReg)
        {
            throw new ValidationException("Hệ thống hiện đang tạm dừng mở đăng ký tài khoản mới theo chính sách của Quản trị viên.");
        }

        if (await userRepo.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ValidationException("Email này đã được sử dụng trên hệ thống.");
        }

        if (await userRepo.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ValidationException("Tên đăng nhập này đã tồn tại.");
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userRepo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign Member Role by default
        var roleRepo = _unitOfWork.Repository<Role>();
        var memberRole = await roleRepo.FirstOrDefaultAsync(r => r.Name == "Member", cancellationToken);
        if (memberRole != null)
        {
            var userRoleRepo = _unitOfWork.Repository<UserRole>();
            await userRoleRepo.AddAsync(new UserRole { UserId = user.Id, RoleId = memberRole.Id }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var roles = memberRole != null ? new List<string> { memberRole.Name } : new List<string>();
        var (accessToken, expiresInSeconds) = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshTokenEntity = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        await refreshTokenRepo.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = MapToUserDto(user, roles);
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds,
            User = userDto
        };

        return (authResponse, refreshTokenEntity.Token);
    }

    public async Task<(AuthResponse AuthResponse, string RefreshToken)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var term = request.EmailOrUsername.Trim().ToLowerInvariant();
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == term || u.Username.ToLower() == term, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException("Tài khoản của bạn đã bị khóa hoặc ngưng hoạt động.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        _unitOfWork.Repository<User>().Update(user);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (accessToken, expiresInSeconds) = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshTokenEntity = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        await refreshTokenRepo.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = MapToUserDto(user, roles);
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds,
            User = userDto
        };

        return (authResponse, refreshTokenEntity.Token);
    }

    public async Task<(AuthResponse AuthResponse, string RefreshToken)> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var existingToken = await refreshTokenRepo.Query()
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            throw new UnauthorizedException("Refresh Token không hợp lệ hoặc đã hết hạn.");
        }

        var user = existingToken.User;
        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException("Tài khoản đã bị ngưng hoạt động.");
        }

        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);
        existingToken.RevokedAt = DateTimeOffset.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        refreshTokenRepo.Update(existingToken);
        await refreshTokenRepo.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var (accessToken, expiresInSeconds) = _jwtTokenGenerator.GenerateAccessToken(user, roles);

        var userDto = MapToUserDto(user, roles);
        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresInSeconds = expiresInSeconds,
            User = userDto
        };

        return (authResponse, newRefreshToken.Token);
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return;

        var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
        var existingToken = await refreshTokenRepo.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (existingToken != null && existingToken.IsActive)
        {
            existingToken.RevokedAt = DateTimeOffset.UtcNow;
            refreshTokenRepo.Update(existingToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return MapToUserDto(user, roles);
    }

    private static UserDto MapToUserDto(User user, List<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Status = user.Status.ToString(),
            Roles = roles,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
