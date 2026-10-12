using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.DTOs.Report;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class AdminManagementService : IAdminManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;

    public AdminManagementService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
    }

    #region Admin Documents

    public async Task ApproveDocumentAsync(long documentId, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        doc.Status = DocumentStatus.Approved;
        doc.PublishedAt = DateTimeOffset.UtcNow;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify Uploader
        await _notificationService.CreateNotificationAsync(
            doc.UserId,
            "Tài liệu đã được phê duyệt",
            $"Tài liệu '{doc.Title}' của bạn đã được phê duyệt thành công và công khai trên hệ thống.",
            NotificationType.DocumentApproved,
            $"/documents/{doc.Slug}",
            cancellationToken);
    }

    public async Task RejectDocumentAsync(long documentId, RejectDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        doc.Status = DocumentStatus.Rejected;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reasonText = !string.IsNullOrWhiteSpace(request.Reason) ? $" Lý do: {request.Reason.Trim()}" : "";

        // Notify Uploader
        await _notificationService.CreateNotificationAsync(
            doc.UserId,
            "Tài liệu bị từ chối",
            $"Tài liệu '{doc.Title}' của bạn không được phê duyệt.{reasonText}",
            NotificationType.DocumentRejected,
            null,
            cancellationToken);
    }

    public async Task HideDocumentAsync(long documentId, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        doc.Status = doc.Status == DocumentStatus.Hidden ? DocumentStatus.Approved : DocumentStatus.Hidden;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task HardDeleteDocumentAsync(long documentId, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        // Delete MinIO File
        if (!string.IsNullOrEmpty(doc.StoredFileName))
        {
            await _fileStorageService.DeleteAsync(doc.StoredFileName, cancellationToken);
        }

        docRepo.Remove(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Admin Users

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(string? searchTerm, string? status, string? role, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            queryable = queryable.Where(u => u.Username.ToLower().Contains(term) || u.Email.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
        {
            queryable = queryable.Where(u => u.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            queryable = queryable.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == role.Trim().ToLower()));
        }

        queryable = queryable.OrderByDescending(u => u.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var users = await queryable
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<AdminUserDto>>(users);
        return new PagedResult<AdminUserDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<AdminUserDto> ToggleUserStatusAsync(long userId, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        user.Status = user.Status == UserStatus.Active ? UserStatus.Inactive : UserStatus.Active;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AdminUserDto>(user);
    }

    public async Task<AdminUserDto> UpdateUserRoleAsync(long userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        var roleRepo = _unitOfWork.Repository<Role>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();

        // Remove old roles
        var existingUserRoles = await userRoleRepo.Query().Where(ur => ur.UserId == userId).ToListAsync(cancellationToken);
        foreach (var ur in existingUserRoles)
        {
            userRoleRepo.Remove(ur);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add new roles
        foreach (var roleName in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var role = await roleRepo.Query().FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.Trim().ToLower(), cancellationToken);
            if (role != null)
            {
                await userRoleRepo.AddAsync(new UserRole { UserId = userId, RoleId = role.Id }, cancellationToken);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await userRepo.Query().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstAsync(u => u.Id == userId, cancellationToken);
        return _mapper.Map<AdminUserDto>(updatedUser);
    }

    public async Task<string> ResetUserPasswordAsync(long userId, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        string tempPassword = "ShareStudy@" + Random.Shared.Next(1000, 9999);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tempPassword;
    }

    public async Task DeleteUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.Query().Include(u => u.Documents).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng", userId);
        }

        if (user.Documents.Any())
        {
            user.Status = UserStatus.Banned;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            userRepo.Update(user);
        }
        else
        {
            userRepo.Remove(user);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Admin Reports

    public async Task<PagedResult<ReportDto>> GetReportsAsync(string? status, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Report>()
            .Query()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReportStatus>(status, true, out var parsedStatus))
        {
            queryable = queryable.Where(r => r.Status == parsedStatus);
        }

        queryable = queryable.OrderByDescending(r => r.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var reports = await queryable
            .Include(r => r.Document)
            .Include(r => r.User)
            .Include(r => r.ResolverUser)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<ReportDto>>(reports);
        return new PagedResult<ReportDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<ReportDto> ResolveReportAsync(long reportId, long adminId, ResolveReportRequest request, CancellationToken cancellationToken = default)
    {
        var reportRepo = _unitOfWork.Repository<Report>();
        var report = await reportRepo.Query()
            .Include(r => r.Document)
            .Include(r => r.User)
            .Include(r => r.ResolverUser)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report == null)
        {
            throw new NotFoundException("Báo cáo", reportId);
        }

        if (!Enum.TryParse<ReportStatus>(request.Status, true, out var parsedStatus))
        {
            throw new ValidationException($"Trạng thái xử lý '{request.Status}' không hợp lệ.");
        }

        report.Status = parsedStatus;
        report.ResolutionNote = request.ResolutionNote?.Trim();
        report.ResolvedBy = adminId;
        report.ResolvedAt = DateTimeOffset.UtcNow;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        reportRepo.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReportDto>(report);
    }

    #endregion

    #region Admin Dashboard Analytics

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalDocuments = await _unitOfWork.Repository<Document>().Query().CountAsync(d => d.Status != DocumentStatus.Deleted, cancellationToken);
        var totalUsers = await _unitOfWork.Repository<User>().Query().CountAsync(cancellationToken);
        var totalDownloads = await _unitOfWork.Repository<Document>().Query().Where(d => d.Status != DocumentStatus.Deleted).SumAsync(d => (int?)d.DownloadCount, cancellationToken) ?? 0;
        var totalViews = await _unitOfWork.Repository<Document>().Query().Where(d => d.Status != DocumentStatus.Deleted).SumAsync(d => (int?)d.ViewCount, cancellationToken) ?? 0;
        var pendingDocuments = await _unitOfWork.Repository<Document>().Query().CountAsync(d => d.Status == DocumentStatus.Pending, cancellationToken);
        var pendingReports = await _unitOfWork.Repository<Report>().Query().CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalDocuments = totalDocuments,
            TotalUsers = totalUsers,
            TotalDownloads = totalDownloads,
            TotalViews = totalViews,
            PendingDocuments = pendingDocuments,
            PendingReports = pendingReports
        };
    }

    public async Task<DashboardChartsDto> GetDashboardChartsAsync(CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        // 1. Documents By Day
        var docsGrouped = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Where(d => d.CreatedAt >= thirtyDaysAgo && d.Status != DocumentStatus.Deleted)
            .GroupBy(d => d.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // 2. Downloads By Day (from DownloadHistory)
        var downloadsGrouped = await _unitOfWork.Repository<DownloadHistory>()
            .Query()
            .AsNoTracking()
            .Where(dh => dh.DownloadedAt >= thirtyDaysAgo)
            .GroupBy(dh => dh.DownloadedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // 3. Users By Day
        var usersGrouped = await _unitOfWork.Repository<User>()
            .Query()
            .AsNoTracking()
            .Where(u => u.CreatedAt >= thirtyDaysAgo)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // 4. Top Subjects by Document Count
        var topSubjects = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Where(d => d.SubjectId.HasValue && d.Status != DocumentStatus.Deleted)
            .GroupBy(d => d.Subject!.Name)
            .Select(g => new ChartDataItemDto { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Top Schools by Document Count
        var topSchools = await _unitOfWork.Repository<School>()
            .Query()
            .AsNoTracking()
            .Select(s => new ChartDataItemDto { Label = s.Name, Value = s.Documents.Count(d => d.Status != DocumentStatus.Deleted) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Build continuous 30-day data points
        var documentsByDay = new List<ChartDataItemDto>();
        var downloadsByDay = new List<ChartDataItemDto>();
        var usersByDay = new List<ChartDataItemDto>();

        for (int i = 29; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).Date;
            var dateStr = date.ToString("dd/MM");

            var docItem = docsGrouped.FirstOrDefault(x => x.Date == date);
            documentsByDay.Add(new ChartDataItemDto { Label = dateStr, Value = docItem?.Count ?? 0 });

            var dlItem = downloadsGrouped.FirstOrDefault(x => x.Date == date);
            downloadsByDay.Add(new ChartDataItemDto { Label = dateStr, Value = dlItem?.Count ?? 0 });

            var userItem = usersGrouped.FirstOrDefault(x => x.Date == date);
            usersByDay.Add(new ChartDataItemDto { Label = dateStr, Value = userItem?.Count ?? 0 });
        }

        return new DashboardChartsDto
        {
            DocumentsByDay = documentsByDay,
            DownloadsByDay = downloadsByDay,
            UsersByDay = usersByDay,
            TopSubjects = topSubjects,
            TopSchools = topSchools
        };
    }

    #endregion

    #region Audit Logs & System Settings

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? keyword, string? module, string? severity, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<AuditLog>().Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            queryable = queryable.Where(x => x.Performer.ToLower().Contains(k) || x.ActionName.ToLower().Contains(k) || x.Details.ToLower().Contains(k) || x.IpAddress.Contains(k));
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            queryable = queryable.Where(x => x.Module.ToLower() == module.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            queryable = queryable.Where(x => x.Severity.ToLower() == severity.Trim().ToLower());
        }

        queryable = queryable.OrderByDescending(x => x.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<AuditLogDto>>(items);
        return new PagedResult<AuditLogDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task LogActivityAsync(string performer, string ipAddress, string actionType, string actionName, string module, string severity, string details, object? payload = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            Performer = performer,
            IpAddress = ipAddress,
            ActionType = actionType,
            ActionName = actionName,
            Module = module,
            Severity = severity,
            Details = details,
            Payload = payload != null ? System.Text.Json.JsonSerializer.Serialize(payload) : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SystemSettingsDto> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settingsList = await _unitOfWork.Repository<SystemSetting>().Query().AsNoTracking().ToListAsync(cancellationToken);
        var dto = new SystemSettingsDto();

        foreach (var s in settingsList)
        {
            switch (s.Key)
            {
                case "SiteName": dto.SiteName = s.Value; break;
                case "SiteSlogan": dto.SiteSlogan = s.Value; break;
                case "ContactEmail": dto.ContactEmail = s.Value; break;
                case "MaintenanceMode": dto.MaintenanceMode = bool.TryParse(s.Value, out var mm) && mm; break;
                case "AllowRegistration": dto.AllowRegistration = bool.TryParse(s.Value, out var ar) && ar; break;
                case "RequireEmailVerification": dto.RequireEmailVerification = bool.TryParse(s.Value, out var rev) && rev; break;
                case "MaxUploadMB": dto.MaxUploadMB = int.TryParse(s.Value, out var mu) ? mu : 50; break;
                case "AllowedFileExtensions":
                    dto.AllowedFileExtensions = s.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    break;
                case "NotifyOnDocumentApproval": dto.NotifyOnDocumentApproval = bool.TryParse(s.Value, out var nda) && nda; break;
                case "NotifyOnNewReport": dto.NotifyOnNewReport = bool.TryParse(s.Value, out var nnr) && nnr; break;
                case "SmtpHost": dto.SmtpHost = s.Value; break;
                case "SmtpPort": dto.SmtpPort = int.TryParse(s.Value, out var sp) ? sp : 587; break;
                case "SmtpEmail": dto.SmtpEmail = s.Value; break;
                case "StorageProvider": dto.StorageProvider = s.Value; break;
                case "StorageBasePath": dto.StorageBasePath = s.Value; break;
            }
        }

        return dto;
    }

    public async Task<SystemSettingsDto> UpdateSystemSettingsAsync(SystemSettingsDto request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<SystemSetting>();
        var existing = await repo.Query().ToListAsync(cancellationToken);

        async Task Upsert(string key, string value)
        {
            var s = existing.FirstOrDefault(x => x.Key == key);
            if (s == null)
            {
                await repo.AddAsync(new SystemSetting { Key = key, Value = value, CreatedAt = DateTimeOffset.UtcNow }, cancellationToken);
            }
            else
            {
                s.Value = value;
                s.UpdatedAt = DateTimeOffset.UtcNow;
                repo.Update(s);
            }
        }

        await Upsert("SiteName", request.SiteName);
        await Upsert("SiteSlogan", request.SiteSlogan);
        await Upsert("ContactEmail", request.ContactEmail);
        await Upsert("MaintenanceMode", request.MaintenanceMode.ToString());
        await Upsert("AllowRegistration", request.AllowRegistration.ToString());
        await Upsert("RequireEmailVerification", request.RequireEmailVerification.ToString());
        await Upsert("MaxUploadMB", request.MaxUploadMB.ToString());
        await Upsert("AllowedFileExtensions", string.Join(",", request.AllowedFileExtensions));
        await Upsert("NotifyOnDocumentApproval", request.NotifyOnDocumentApproval.ToString());
        await Upsert("NotifyOnNewReport", request.NotifyOnNewReport.ToString());
        await Upsert("SmtpHost", request.SmtpHost);
        await Upsert("SmtpPort", request.SmtpPort.ToString());
        await Upsert("SmtpEmail", request.SmtpEmail);
        await Upsert("StorageProvider", request.StorageProvider);
        await Upsert("StorageBasePath", request.StorageBasePath);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request;
    }

    #endregion
}
