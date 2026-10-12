using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Rating;

namespace ShareStudy.Application.Interfaces;

public interface IRatingService
{
    Task<PagedResult<RatingDto>> GetDocumentRatingsAsync(long documentId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<RatingDto> CreateRatingAsync(long userId, long documentId, CreateRatingRequest request, CancellationToken cancellationToken = default);
    Task<RatingDto> UpdateRatingAsync(long userId, long documentId, long ratingId, UpdateRatingRequest request, CancellationToken cancellationToken = default);
    Task DeleteRatingAsync(long userId, long documentId, long ratingId, bool isAdmin, CancellationToken cancellationToken = default);
}
