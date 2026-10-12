using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Comment;

namespace ShareStudy.Application.Interfaces;

public interface ICommentService
{
    Task<PagedResult<CommentDto>> GetDocumentCommentsAsync(long documentId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<CommentDto> CreateCommentAsync(long userId, long documentId, CreateCommentRequest request, CancellationToken cancellationToken = default);
    Task<CommentDto> UpdateCommentAsync(long userId, long commentId, UpdateCommentRequest request, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(long userId, long commentId, bool isAdmin, CancellationToken cancellationToken = default);
}
