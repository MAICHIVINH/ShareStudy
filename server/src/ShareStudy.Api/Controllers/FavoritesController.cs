using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Favorite;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ICurrentUserService _currentUserService;

    public FavoritesController(IFavoriteService favoriteService, ICurrentUserService currentUserService)
    {
        _favoriteService = favoriteService;
        _currentUserService = currentUserService;
    }

    [HttpGet("api/users/me/favorites")]
    public async Task<ActionResult<ApiResponse<PagedResult<FavoriteDto>>>> GetMyFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _favoriteService.GetUserFavoritesAsync(userId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<FavoriteDto>>.SuccessResponse(result));
    }

    [HttpPost("api/favorites/{documentId}")]
    public async Task<ActionResult<ApiResponse>> AddFavorite(
        long documentId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        await _favoriteService.AddFavoriteAsync(userId, documentId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Thêm vào danh sách yêu thích thành công"));
    }

    [HttpDelete("api/favorites/{documentId}")]
    public async Task<ActionResult<ApiResponse>> RemoveFavorite(
        long documentId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        await _favoriteService.RemoveFavoriteAsync(userId, documentId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Bỏ yêu thích thành công"));
    }
}
