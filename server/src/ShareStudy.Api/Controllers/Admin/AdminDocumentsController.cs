using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Admin;
using ShareStudy.Application.DTOs.Document;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/documents")]
[Authorize(Policy = "ModeratorOrAdmin")]
public class AdminDocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IAdminManagementService _adminService;

    public AdminDocumentsController(IDocumentService documentService, IAdminManagementService adminService)
    {
        _documentService = documentService;
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DocumentListItemDto>>>> GetAdminDocuments(
        [FromQuery] DocumentQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.GetDocumentsAsync(query, null, true, cancellationToken);
        return Ok(ApiResponse<PagedResult<DocumentListItemDto>>.SuccessResponse(result));
    }

    [HttpPut("{id}/approve")]
    [HttpPatch("{id}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveDocument(long id, CancellationToken cancellationToken)
    {
        await _adminService.ApproveDocumentAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Phê duyệt tài liệu thành công"));
    }

    [HttpPut("{id}/reject")]
    [HttpPatch("{id}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectDocument(
        long id,
        [FromBody] RejectDocumentRequest request,
        CancellationToken cancellationToken)
    {
        await _adminService.RejectDocumentAsync(id, request, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Từ chối tài liệu thành công"));
    }

    [HttpPut("{id}/hide")]
    [HttpPatch("{id}/hide")]
    public async Task<ActionResult<ApiResponse>> HideDocument(long id, CancellationToken cancellationToken)
    {
        await _adminService.HideDocumentAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Cập nhật trạng thái hiển thị tài liệu thành công"));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse>> HardDeleteDocument(long id, CancellationToken cancellationToken)
    {
        await _adminService.HardDeleteDocumentAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Xóa vĩnh viễn tài liệu thành công"));
    }
}
