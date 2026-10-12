using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Auth;
using ShareStudy.Application.DTOs.User;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public UserService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || user.Status == UserStatus.Banned)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        user.Bio = request.Bio?.Trim();

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
        {
            user.AvatarUrl = request.AvatarUrl.Trim();
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        return dto;
    }

    public async Task<string> UploadAvatarAsync(long userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException(new[] { "File ảnh đại diện không hợp lệ" });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new ValidationException(new[] { "Dung lượng ảnh đại diện tối đa là 10MB" });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            throw new ValidationException(new[] { "Định dạng file không hỗ trợ. Vui lòng chọn ảnh JPG, PNG, WEBP, GIF." });
        }

        using var stream = file.OpenReadStream();
        var storedFileName = await _fileStorageService.UploadAsync(stream, file.FileName, file.ContentType, cancellationToken);

        // Build accessible Storage Proxy URL via API server (port 5068)
        var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5068";
        var publicUrl = $"{apiBaseUrl.TrimEnd('/')}/api/storage/{storedFileName}";

        return publicUrl;
    }
}
