using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Document;

namespace ShareStudy.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDetailDto> CreateDocumentAsync(long userId, CreateDocumentRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<DocumentListItemDto>> GetDocumentsAsync(DocumentQueryParameters query, long? currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> GetDocumentByIdAsync(long id, long? currentUserId, string? clientIpAddress, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> GetDocumentBySlugAsync(string slug, long? currentUserId, string? clientIpAddress, CancellationToken cancellationToken = default);
    Task<DocumentDetailDto> UpdateDocumentAsync(long id, long currentUserId, bool isAdmin, UpdateDocumentRequest request, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(long id, long currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<DocumentDownloadDto> DownloadDocumentAsync(long id, long? currentUserId, string? clientIpAddress, string? userAgent, CancellationToken cancellationToken = default);
    Task<DocumentPreviewDto> PreviewDocumentAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentListItemDto>> GetPopularDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentListItemDto>> GetLatestDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentListItemDto>> GetTopRatedDocumentsAsync(int count = 10, long? currentUserId = null, CancellationToken cancellationToken = default);
}
