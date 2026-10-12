using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Favorite;

namespace ShareStudy.Application.Interfaces;

public interface IFavoriteService
{
    Task<PagedResult<FavoriteDto>> GetUserFavoritesAsync(long userId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task AddFavoriteAsync(long userId, long documentId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteAsync(long userId, long documentId, CancellationToken cancellationToken = default);
}
