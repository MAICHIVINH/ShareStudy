using Microsoft.AspNetCore.Http;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Application.DTOs.User;

namespace ShareStudy.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<string> UploadAvatarAsync(long userId, IFormFile file, CancellationToken cancellationToken = default);
}
