using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Document;
using ShareStudy.Application.Interfaces;
using CustomValidationException = ShareStudy.Application.Common.Exceptions.ValidationException;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(IDocumentService documentService, ICurrentUserService currentUserService)
    {
        _documentService = documentService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> CreateDocument(
        [FromForm] CreateDocumentRequest request,
        [FromServices] IValidator<CreateDocumentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var result = await _documentService.CreateDocumentAsync(userId, request, cancellationToken);
        return StatusCode(201, ApiResponse<DocumentDetailDto>.SuccessResponse(result, "Đăng tài liệu thành công, trạng thái: Đang chờ duyệt", 201));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DocumentListItemDto>>>> GetDocuments(
        [FromQuery] DocumentQueryParameters query,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var isAdmin = _currentUserService.Roles.Contains("Admin") || _currentUserService.Roles.Contains("Moderator");
        var result = await _documentService.GetDocumentsAsync(query, currentUserId, isAdmin, cancellationToken);
        return Ok(ApiResponse<PagedResult<DocumentListItemDto>>.SuccessResponse(result));
    }

    [HttpGet("popular")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentListItemDto>>>> GetPopularDocuments(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var result = await _documentService.GetPopularDocumentsAsync(count, currentUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentListItemDto>>.SuccessResponse(result));
    }

    [HttpGet("latest")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentListItemDto>>>> GetLatestDocuments(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var result = await _documentService.GetLatestDocumentsAsync(count, currentUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentListItemDto>>.SuccessResponse(result));
    }

    [HttpGet("top-rated")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DocumentListItemDto>>>> GetTopRatedDocuments(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var result = await _documentService.GetTopRatedDocumentsAsync(count, currentUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DocumentListItemDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> GetDocumentById(
        long id,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _documentService.GetDocumentByIdAsync(id, currentUserId, clientIp, cancellationToken);
        return Ok(ApiResponse<DocumentDetailDto>.SuccessResponse(result));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> GetDocumentBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _documentService.GetDocumentBySlugAsync(slug, currentUserId, clientIp, cancellationToken);
        return Ok(ApiResponse<DocumentDetailDto>.SuccessResponse(result));
    }

    [HttpPut("{id}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> UpdateDocument(
        long id,
        [FromForm] UpdateDocumentRequest request,
        [FromServices] IValidator<UpdateDocumentRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new CustomValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var userId = _currentUserService.UserId ?? 0;
        var isAdmin = _currentUserService.Roles.Contains("Admin");
        var result = await _documentService.UpdateDocumentAsync(id, userId, isAdmin, request, cancellationToken);
        return Ok(ApiResponse<DocumentDetailDto>.SuccessResponse(result, "Cập nhật tài liệu thành công"));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteDocument(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        var isAdmin = _currentUserService.Roles.Contains("Admin");
        await _documentService.DeleteDocumentAsync(id, userId, isAdmin, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa tài liệu thành công"));
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult<ApiResponse<DocumentDownloadDto>>> DownloadDocument(
        long id,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await _documentService.DownloadDocumentAsync(id, currentUserId, clientIp, userAgent, cancellationToken);
        return Ok(ApiResponse<DocumentDownloadDto>.SuccessResponse(result, "Lấy link tải tài liệu thành công"));
    }

    [HttpGet("{id}/preview")]
    public async Task<ActionResult<ApiResponse<DocumentPreviewDto>>> PreviewDocument(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.PreviewDocumentAsync(id, cancellationToken);
        return Ok(ApiResponse<DocumentPreviewDto>.SuccessResponse(result));
    }

    [HttpPost("{id}/view")]
    public ActionResult<ApiResponse> RecordView(long id)
    {
        return Ok(ApiResponse.SuccessResponse("Ghi nhận lượt xem thành công"));
    }
}
