using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Comment;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ICurrentUserService _currentUserService;

    public CommentsController(ICommentService commentService, ICurrentUserService currentUserService)
    {
        _commentService = commentService;
        _currentUserService = currentUserService;
    }

    [HttpGet("api/documents/{id}/comments")]
    public async Task<ActionResult<ApiResponse<PagedResult<CommentDto>>>> GetComments(
        long id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetDocumentCommentsAsync(id, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<CommentDto>>.SuccessResponse(result));
    }

    [HttpPost("api/documents/{id}/comments")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(
        long id,
        [FromBody] CreateCommentRequest request,
        [FromServices] IValidator<CreateCommentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _commentService.CreateCommentAsync(userId, id, request, cancellationToken);
        return StatusCode(201, ApiResponse<CommentDto>.SuccessResponse(result, "Thêm bình luận thành công", 201));
    }

    [HttpPut("api/comments/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommentDto>>> UpdateComment(
        long id,
        [FromBody] UpdateCommentRequest request,
        [FromServices] IValidator<UpdateCommentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _commentService.UpdateCommentAsync(userId, id, request, cancellationToken);
        return Ok(ApiResponse<CommentDto>.SuccessResponse(result, "Cập nhật bình luận thành công"));
    }

    [HttpDelete("api/comments/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteComment(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        var isAdmin = _currentUserService.Roles.Contains("Admin");
        await _commentService.DeleteCommentAsync(userId, id, isAdmin, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa bình luận thành công"));
    }
}
