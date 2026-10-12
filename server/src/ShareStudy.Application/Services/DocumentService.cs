using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Document;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentDetailDto> CreateDocumentAsync(long userId, CreateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Validate File Upload & Magic Bytes
        await FileValidationHelper.ValidateFileAsync(request.File, cancellationToken);

        // Check dynamic SystemSettings for File Limit and Extensions
        var settingRepo = _unitOfWork.Repository<SystemSetting>();
        var maxUploadSetting = await settingRepo.Query().FirstOrDefaultAsync(s => s.Key == "MaxUploadMB", cancellationToken);
        int maxMb = maxUploadSetting != null && int.TryParse(maxUploadSetting.Value, out var m) ? m : 50;
        if (request.File.Length > (long)maxMb * 1024 * 1024)
        {
            throw new ValidationException($"Dung lượng file vượt quá giới hạn tối đa của hệ thống ({maxMb}MB).");
        }

        var allowedExtSetting = await settingRepo.Query().FirstOrDefaultAsync(s => s.Key == "AllowedFileExtensions", cancellationToken);
        if (allowedExtSetting != null && !string.IsNullOrWhiteSpace(allowedExtSetting.Value))
        {
            var allowedExts = allowedExtSetting.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLowerInvariant());
            var fileExt = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
            if (!allowedExts.Contains(fileExt))
            {
                throw new ValidationException($"Định dạng file .{fileExt} không được phép tải lên. Các định dạng được chấp nhận: {allowedExtSetting.Value}");
            }
        }

        // 2. Validate Foreign Keys
        var eduLevel = await _unitOfWork.Repository<EducationLevel>().GetByIdAsync(request.EducationLevelId, cancellationToken);
        if (eduLevel == null) throw new NotFoundException("Cấp học", request.EducationLevelId);

        var docType = await _unitOfWork.Repository<DocumentType>().GetByIdAsync(request.DocumentTypeId, cancellationToken);
        if (docType == null) throw new NotFoundException("Loại tài liệu", request.DocumentTypeId);

        // 3. Upload File to MinIO
        var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var fileType = ext.TrimStart('.');
        using var stream = request.File.OpenReadStream();
        var storedFileName = await _fileStorageService.UploadAsync(stream, request.File.FileName, request.File.ContentType, cancellationToken);

        // 4. Generate Slug
        var slug = GenerateSlug(request.Title);
        var docRepo = _unitOfWork.Repository<Document>();
        if (await docRepo.AnyAsync(d => d.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        // 5. Create Document Entity
        var document = new Document
        {
            Title = request.Title.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            FileName = request.File.FileName,
            StoredFileName = storedFileName,
            FileType = fileType,
            FileSize = request.File.Length,
            MimeType = request.File.ContentType,
            EducationLevelId = request.EducationLevelId,
            SchoolId = request.SchoolId,
            FacultyId = request.FacultyId,
            MajorId = request.MajorId,
            SubjectId = request.SubjectId,
            DocumentTypeId = request.DocumentTypeId,
            AcademicYearId = request.AcademicYearId,
            UserId = userId,
            Status = DocumentStatus.Pending,
            PublishedAt = DateTimeOffset.UtcNow
        };

        await docRepo.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Handle Tags
        if (request.Tags != null && request.Tags.Any())
        {
            var tagRepo = _unitOfWork.Repository<Tag>();
            var docTagRepo = _unitOfWork.Repository<DocumentTag>();

            foreach (var tagName in request.Tags.Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var sluggedTag = GenerateSlug(tagName);
                var tag = await tagRepo.Query().FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower(), cancellationToken);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName, Slug = sluggedTag };
                    await tagRepo.AddAsync(tag, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                await docTagRepo.AddAsync(new DocumentTag
                {
                    DocumentId = document.Id,
                    TagId = tag.Id
                }, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await GetDocumentDetailByIdAsync(document.Id, userId, cancellationToken);
    }

    public async Task<PagedResult<DocumentListItemDto>> GetDocumentsAsync(DocumentQueryParameters query, long? currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking();

        // 1. UserId & Status Filter
        if (query.UserId.HasValue)
        {
            queryable = queryable.Where(d => d.UserId == query.UserId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<DocumentStatus>(query.Status, true, out var parsedStatus))
            {
                queryable = queryable.Where(d => d.Status == parsedStatus);
            }
            else if (!isAdmin && query.UserId.Value != (currentUserId ?? 0))
            {
                queryable = queryable.Where(d => d.Status == DocumentStatus.Approved);
            }
            else
            {
                queryable = queryable.Where(d => d.Status != DocumentStatus.Deleted);
            }
        }
        else
        {
            // General catalog listing
            if (isAdmin)
            {
                if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<DocumentStatus>(query.Status, true, out var parsedStatus))
                {
                    queryable = queryable.Where(d => d.Status == parsedStatus);
                }
                else
                {
                    queryable = queryable.Where(d => d.Status != DocumentStatus.Deleted);
                }
            }
            else
            {
                queryable = queryable.Where(d => d.Status == DocumentStatus.Approved);
            }
        }

        // 2. Keyword Search
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToLower();
            queryable = queryable.Where(d =>
                d.Title.ToLower().Contains(keyword) ||
                (d.Subject != null && d.Subject.Name.ToLower().Contains(keyword)) ||
                (d.School != null && d.School.Name.ToLower().Contains(keyword)) ||
                (d.Faculty != null && d.Faculty.Name.ToLower().Contains(keyword)) ||
                (d.Major != null && d.Major.Name.ToLower().Contains(keyword)) ||
                d.User.FullName.ToLower().Contains(keyword) ||
                d.DocumentTags.Any(dt => dt.Tag.Name.ToLower().Contains(keyword)));
        }

        // 3. Category & Filter Criteria
        if (query.EducationLevelId.HasValue) queryable = queryable.Where(d => d.EducationLevelId == query.EducationLevelId.Value);
        if (query.SchoolId.HasValue) queryable = queryable.Where(d => d.SchoolId == query.SchoolId.Value);
        if (query.FacultyId.HasValue) queryable = queryable.Where(d => d.FacultyId == query.FacultyId.Value);
        if (query.MajorId.HasValue) queryable = queryable.Where(d => d.MajorId == query.MajorId.Value);
        if (query.SubjectId.HasValue) queryable = queryable.Where(d => d.SubjectId == query.SubjectId.Value);
        if (query.DocumentTypeId.HasValue) 
        {
            queryable = queryable.Where(d => d.DocumentTypeId == query.DocumentTypeId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(query.DocumentTypeName))
        {
            var typeName = query.DocumentTypeName.Trim().ToLower();
            queryable = queryable.Where(d => d.DocumentType != null && d.DocumentType.Name.ToLower() == typeName);
        }
        if (query.AcademicYearId.HasValue) queryable = queryable.Where(d => d.AcademicYearId == query.AcademicYearId.Value);

        if (!string.IsNullOrWhiteSpace(query.FileType))
        {
            var formats = query.FileType.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(f => f.Trim().ToLower())
                                        .ToList();
            if (formats.Count > 0)
            {
                queryable = queryable.Where(d => formats.Contains(d.FileType.ToLower()));
            }
        }

        if (query.MinRating.HasValue)
        {
            var minRatingDecimal = (decimal)query.MinRating.Value;
            queryable = queryable.Where(d => d.AverageRating >= minRatingDecimal);
        }

        // 4. Sorting
        var sortOption = (query.Sort ?? query.SortBy)?.Trim().ToLower().Replace("_", "-");
        queryable = sortOption switch
        {
            "oldest" => queryable.OrderBy(d => d.CreatedAt),
            "most-downloaded" => queryable.OrderByDescending(d => d.DownloadCount),
            "highest-rated" => queryable.OrderByDescending(d => d.AverageRating).ThenByDescending(d => d.RatingCount),
            "most-viewed" => queryable.OrderByDescending(d => d.ViewCount),
            "title-asc" or "name-asc" => queryable.OrderBy(d => d.Title),
            "title-desc" or "name-desc" => queryable.OrderByDescending(d => d.Title),
            _ => queryable.OrderByDescending(d => d.CreatedAt)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .Include(d => d.EducationLevel)
            .Include(d => d.School)
            .Include(d => d.Faculty)
            .Include(d => d.Major)
            .Include(d => d.Subject)
            .Include(d => d.User)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<DocumentListItemDto>>(items);
        await PopulateIsFavoriteAsync(dtos, currentUserId, cancellationToken);

        return new PagedResult<DocumentListItemDto>(dtos, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<DocumentDetailDto> GetDocumentByIdAsync(long id, long? currentUserId, string? clientIpAddress, CancellationToken cancellationToken = default)
    {
        var doc = await _unitOfWork.Repository<Document>()
            .Query()
            .FirstOrDefaultAsync(d => d.Id == id && d.Status != DocumentStatus.Deleted, cancellationToken);

        if (doc == null)
        {
            throw new NotFoundException("Tài liệu", id);
        }

        // Record View
        var viewRepo = _unitOfWork.Repository<View>();
        var thirtyMinsAgo = DateTimeOffset.UtcNow.AddMinutes(-30);
        System.Net.IPAddress? parsedIp = System.Net.IPAddress.TryParse(clientIpAddress, out var ip) ? ip : null;

        bool alreadyViewedRecently = await viewRepo.AnyAsync(v =>
            v.DocumentId == id &&
            v.ViewedAt >= thirtyMinsAgo &&
            ((currentUserId.HasValue && v.UserId == currentUserId.Value) ||
             (parsedIp != null && v.IpAddress == parsedIp)), cancellationToken);

        if (!alreadyViewedRecently)
        {
            await viewRepo.AddAsync(new View
            {
                DocumentId = id,
                UserId = currentUserId,
                IpAddress = parsedIp,
                ViewedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            doc.ViewCount += 1;
            _unitOfWork.Repository<Document>().Update(doc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await GetDocumentDetailByIdAsync(id, currentUserId, cancellationToken);
    }

    public async Task<DocumentDetailDto> GetDocumentBySlugAsync(string slug, long? currentUserId, string? clientIpAddress, CancellationToken cancellationToken = default)
    {
        var isNumeric = long.TryParse(slug, out var parsedId);

        var doc = await _unitOfWork.Repository<Document>()
            .Query()
            .FirstOrDefaultAsync(d => (d.Slug == slug || (isNumeric && d.Id == parsedId)) && d.Status != DocumentStatus.Deleted, cancellationToken);

        if (doc == null)
        {
            throw new NotFoundException("Tài liệu", slug);
        }

        // Record View
        var viewRepo = _unitOfWork.Repository<View>();
        var thirtyMinsAgo = DateTimeOffset.UtcNow.AddMinutes(-30);
        System.Net.IPAddress? parsedIp = System.Net.IPAddress.TryParse(clientIpAddress, out var ip) ? ip : null;

        bool alreadyViewedRecently = await viewRepo.AnyAsync(v =>
            v.DocumentId == doc.Id &&
            v.ViewedAt >= thirtyMinsAgo &&
            ((currentUserId.HasValue && v.UserId == currentUserId.Value) ||
             (parsedIp != null && v.IpAddress == parsedIp)), cancellationToken);

        if (!alreadyViewedRecently)
        {
            await viewRepo.AddAsync(new View
            {
                DocumentId = doc.Id,
                UserId = currentUserId,
                IpAddress = parsedIp,
                ViewedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            doc.ViewCount += 1;
            _unitOfWork.Repository<Document>().Update(doc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await GetDocumentDetailByIdAsync(doc.Id, currentUserId, cancellationToken);
    }

    public async Task<DocumentDetailDto> UpdateDocumentAsync(long id, long currentUserId, bool isAdmin, UpdateDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(id, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", id);
        }

        if (doc.UserId != currentUserId && !isAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền chỉnh sửa tài liệu này.");
        }

        if (request.File != null)
        {
            await FileValidationHelper.ValidateFileAsync(request.File, cancellationToken);
            await _fileStorageService.DeleteAsync(doc.StoredFileName, cancellationToken);

            using var stream = request.File.OpenReadStream();
            var storedFileName = await _fileStorageService.UploadAsync(stream, request.File.FileName, request.File.ContentType, cancellationToken);

            doc.FileName = request.File.FileName;
            doc.StoredFileName = storedFileName;
            doc.FileType = Path.GetExtension(request.File.FileName).TrimStart('.').ToLower();
            doc.FileSize = request.File.Length;
            doc.MimeType = request.File.ContentType;
            doc.Status = DocumentStatus.Pending;
        }

        if (doc.Title != request.Title.Trim())
        {
            doc.Title = request.Title.Trim();
            doc.Slug = GenerateSlug(doc.Title);
            doc.Status = DocumentStatus.Pending;
        }

        doc.Description = request.Description?.Trim();
        doc.EducationLevelId = request.EducationLevelId;
        doc.SchoolId = request.SchoolId;
        doc.FacultyId = request.FacultyId;
        doc.MajorId = request.MajorId;
        doc.SubjectId = request.SubjectId;
        doc.DocumentTypeId = request.DocumentTypeId;
        doc.AcademicYearId = request.AcademicYearId;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetDocumentDetailByIdAsync(id, currentUserId, cancellationToken);
    }

    public async Task DeleteDocumentAsync(long id, long currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(id, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", id);
        }

        if (doc.UserId != currentUserId && !isAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền xóa tài liệu này.");
        }

        doc.Status = DocumentStatus.Deleted;
        doc.UpdatedAt = DateTimeOffset.UtcNow;

        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DocumentDownloadDto> DownloadDocumentAsync(long id, long? currentUserId, string? clientIpAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(id, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", id);
        }

        if (doc.Status != DocumentStatus.Approved && (!currentUserId.HasValue || doc.UserId != currentUserId.Value))
        {
            throw new BadRequestException("Tài liệu chưa được phê duyệt nên không thể tải về.");
        }

        var downloadHistoryRepo = _unitOfWork.Repository<DownloadHistory>();
        System.Net.IPAddress? parsedIp = System.Net.IPAddress.TryParse(clientIpAddress, out var ip) ? ip : null;

        await downloadHistoryRepo.AddAsync(new DownloadHistory
        {
            DocumentId = id,
            UserId = currentUserId,
            IpAddress = parsedIp,
            UserAgent = userAgent,
            DownloadedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        doc.DownloadCount += 1;
        docRepo.Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var presignedUrl = await _fileStorageService.GetPresignedUrlAsync(doc.StoredFileName, 3600, cancellationToken);

        return new DocumentDownloadDto
        {
            DocumentId = doc.Id,
            Title = doc.Title,
            FileName = doc.FileName,
            DownloadUrl = presignedUrl
        };
    }

    public async Task<DocumentPreviewDto> PreviewDocumentAsync(long id, CancellationToken cancellationToken = default)
    {
        var doc = await _unitOfWork.Repository<Document>().GetByIdAsync(id, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", id);
        }

        var isPdf = doc.FileType.ToLower() == "pdf" || doc.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdf)
        {
            return new DocumentPreviewDto
            {
                DocumentId = doc.Id,
                Title = doc.Title,
                IsSupported = false,
                Message = "Định dạng file không hỗ trợ xem trực tuyến, vui lòng tải về để xem."
            };
        }

        var presignedUrl = await _fileStorageService.GetPresignedUrlAsync(doc.StoredFileName, 3600, cancellationToken);
        return new DocumentPreviewDto
        {
            DocumentId = doc.Id,
            Title = doc.Title,
            IsSupported = true,
            PreviewUrl = presignedUrl,
            Message = "Thành công"
        };
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetPopularDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var docs = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Where(d => d.Status == DocumentStatus.Approved)
            .Include(d => d.EducationLevel)
            .Include(d => d.School)
            .Include(d => d.Faculty)
            .Include(d => d.Major)
            .Include(d => d.Subject)
            .Include(d => d.User)
            .OrderByDescending(d => d.DownloadCount)
            .Take(count)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<DocumentListItemDto>>(docs);
        await PopulateIsFavoriteAsync(dtos, currentUserId, cancellationToken);
        return dtos;
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetLatestDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var docs = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Where(d => d.Status == DocumentStatus.Approved)
            .Include(d => d.EducationLevel)
            .Include(d => d.School)
            .Include(d => d.Faculty)
            .Include(d => d.Major)
            .Include(d => d.Subject)
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<DocumentListItemDto>>(docs);
        await PopulateIsFavoriteAsync(dtos, currentUserId, cancellationToken);
        return dtos;
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetTopRatedDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default)
    {
        var docs = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Where(d => d.Status == DocumentStatus.Approved)
            .Include(d => d.EducationLevel)
            .Include(d => d.School)
            .Include(d => d.Faculty)
            .Include(d => d.Major)
            .Include(d => d.Subject)
            .Include(d => d.User)
            .OrderByDescending(d => d.AverageRating)
            .ThenByDescending(d => d.RatingCount)
            .Take(count)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<DocumentListItemDto>>(docs);
        await PopulateIsFavoriteAsync(dtos, currentUserId, cancellationToken);
        return dtos;
    }

    private async Task<DocumentDetailDto> GetDocumentDetailByIdAsync(long id, long? currentUserId, CancellationToken cancellationToken)
    {
        var doc = await _unitOfWork.Repository<Document>()
            .Query()
            .AsNoTracking()
            .Include(d => d.EducationLevel)
            .Include(d => d.School)
            .Include(d => d.Faculty)
            .Include(d => d.Major)
            .Include(d => d.Subject)
            .Include(d => d.DocumentType)
            .Include(d => d.AcademicYear)
            .Include(d => d.User)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doc == null) throw new NotFoundException("Tài liệu", id);

        var dto = _mapper.Map<DocumentDetailDto>(doc);
        if (doc.FileType.ToLower() == "pdf")
        {
            dto.PresignedPreviewUrl = await _fileStorageService.GetPresignedUrlAsync(doc.StoredFileName, 3600, cancellationToken);
        }

        if (currentUserId.HasValue)
        {
            var isFav = await _unitOfWork.Repository<Favorite>()
                .Query()
                .AsNoTracking()
                .AnyAsync(f => f.UserId == currentUserId.Value && f.DocumentId == id, cancellationToken);
            dto.IsFavorite = isFav;
        }

        return dto;
    }

    private async Task PopulateIsFavoriteAsync(IEnumerable<DocumentListItemDto> dtos, long? currentUserId, CancellationToken cancellationToken)
    {
        if (!currentUserId.HasValue || dtos == null || !dtos.Any()) return;

        var docIds = dtos.Select(d => d.Id).ToList();
        var favoritedDocIds = await _unitOfWork.Repository<Favorite>()
            .Query()
            .AsNoTracking()
            .Where(f => f.UserId == currentUserId.Value && docIds.Contains(f.DocumentId))
            .Select(f => f.DocumentId)
            .ToListAsync(cancellationToken);

        var favoritedSet = new HashSet<long>(favoritedDocIds);
        foreach (var dto in dtos)
        {
            dto.IsFavorite = favoritedSet.Contains(dto.Id);
        }
    }

    private static string GenerateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var unaccented = RemoveDiacritics(text.Trim().ToLowerInvariant());
        var slug = Regex.Replace(unaccented, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "tai-lieu" : slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
