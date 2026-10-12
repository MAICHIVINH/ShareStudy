using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Rating;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/documents/{id}/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly ICurrentUserService _currentUserService;

    public RatingsController(IRatingService ratingService, ICurrentUserService currentUserService)
    {
        _ratingService = ratingService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RatingDto>>>> GetRatings(
        long id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _ratingService.GetDocumentRatingsAsync(id, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<RatingDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RatingDto>>> CreateRating(
        long id,
        [FromBody] CreateRatingRequest request,
        [FromServices] IValidator<CreateRatingRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _ratingService.CreateRatingAsync(userId, id, request, cancellationToken);
        return StatusCode(201, ApiResponse<RatingDto>.SuccessResponse(result, "Đánh giá tài liệu thành công", 201));
    }

    [HttpPut("{ratingId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RatingDto>>> UpdateRating(
        long id,
        long ratingId,
        [FromBody] UpdateRatingRequest request,
        [FromServices] IValidator<UpdateRatingRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _ratingService.UpdateRatingAsync(userId, id, ratingId, request, cancellationToken);
        return Ok(ApiResponse<RatingDto>.SuccessResponse(result, "Cập nhật đánh giá thành công"));
    }

    [HttpDelete("{ratingId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteRating(
        long id,
        long ratingId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        var isAdmin = _currentUserService.Roles.Contains("Admin");
        await _ratingService.DeleteRatingAsync(userId, id, ratingId, isAdmin, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa đánh giá thành công"));
    }
}
