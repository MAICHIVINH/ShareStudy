using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Rating;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class RatingService : IRatingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public RatingService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<RatingDto>> GetDocumentRatingsAsync(long documentId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Rating>()
            .Query()
            .AsNoTracking()
            .Where(r => r.DocumentId == documentId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<RatingDto>>(items);
        return new PagedResult<RatingDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<RatingDto> CreateRatingAsync(long userId, long documentId, CreateRatingRequest request, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        var ratingRepo = _unitOfWork.Repository<Rating>();
        if (await ratingRepo.AnyAsync(r => r.DocumentId == documentId && r.UserId == userId, cancellationToken))
        {
            throw new ValidationException("Bạn đã đánh giá tài liệu này rồi.");
        }

        var rating = new Rating
        {
            DocumentId = documentId,
            UserId = userId,
            Score = request.Score,
            Comment = request.Comment?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await ratingRepo.AddAsync(rating, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recalculate Document AverageRating & RatingCount in transaction
        await RecalculateDocumentRatingAsync(doc, cancellationToken);

        // Notify uploader if rater is not uploader
        if (doc.UserId != userId)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
            var raterName = user?.FullName ?? "Một người dùng";
            await _notificationService.CreateNotificationAsync(
                doc.UserId,
                "Đánh giá mới",
                $"{raterName} đã đánh giá {request.Score} sao cho tài liệu '{doc.Title}'.",
                NotificationType.NewRating,
                $"/documents/{doc.Slug}",
                cancellationToken);
        }

        var created = await ratingRepo.Query().Include(r => r.User).FirstAsync(r => r.Id == rating.Id, cancellationToken);
        return _mapper.Map<RatingDto>(created);
    }

    public async Task<RatingDto> UpdateRatingAsync(long userId, long documentId, long ratingId, UpdateRatingRequest request, CancellationToken cancellationToken = default)
    {
        var ratingRepo = _unitOfWork.Repository<Rating>();
        var rating = await ratingRepo.Query().Include(r => r.User).FirstOrDefaultAsync(r => r.Id == ratingId && r.DocumentId == documentId, cancellationToken);
        if (rating == null)
        {
            throw new NotFoundException("Đánh giá", ratingId);
        }

        if (rating.UserId != userId)
        {
            throw new ForbiddenException("Bạn không có quyền sửa đánh giá này.");
        }

        rating.Score = request.Score;
        rating.Comment = request.Comment?.Trim();
        rating.UpdatedAt = DateTimeOffset.UtcNow;

        ratingRepo.Update(rating);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var doc = await _unitOfWork.Repository<Document>().GetByIdAsync(documentId, cancellationToken);
        if (doc != null)
        {
            await RecalculateDocumentRatingAsync(doc, cancellationToken);
        }

        return _mapper.Map<RatingDto>(rating);
    }

    public async Task DeleteRatingAsync(long userId, long documentId, long ratingId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var ratingRepo = _unitOfWork.Repository<Rating>();
        var rating = await ratingRepo.GetByIdAsync(ratingId, cancellationToken);
        if (rating == null || rating.DocumentId != documentId)
        {
            throw new NotFoundException("Đánh giá", ratingId);
        }

        if (rating.UserId != userId && !isAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền xóa đánh giá này.");
        }

        ratingRepo.Remove(rating);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var doc = await _unitOfWork.Repository<Document>().GetByIdAsync(documentId, cancellationToken);
        if (doc != null)
        {
            await RecalculateDocumentRatingAsync(doc, cancellationToken);
        }
    }

    private async Task RecalculateDocumentRatingAsync(Document doc, CancellationToken cancellationToken)
    {
        var ratings = await _unitOfWork.Repository<Rating>()
            .Query()
            .Where(r => r.DocumentId == doc.Id)
            .ToListAsync(cancellationToken);

        doc.RatingCount = ratings.Count;
        doc.AverageRating = ratings.Count > 0 ? (decimal)ratings.Average(r => r.Score) : 0;

        _unitOfWork.Repository<Document>().Update(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
