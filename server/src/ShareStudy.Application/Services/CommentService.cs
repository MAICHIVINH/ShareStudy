using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Comment;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public CommentService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<CommentDto>> GetDocumentCommentsAsync(long documentId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var rootQueryable = _unitOfWork.Repository<Comment>()
            .Query()
            .AsNoTracking()
            .Where(c => c.DocumentId == documentId && c.ParentId == null && c.Status != CommentStatus.Deleted)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await rootQueryable.CountAsync(cancellationToken);
        var rootComments = await rootQueryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        // Fetch replies for these root comments
        var rootIds = rootComments.Select(c => c.Id).ToList();
        var replies = await _unitOfWork.Repository<Comment>()
            .Query()
            .AsNoTracking()
            .Where(c => c.DocumentId == documentId && c.ParentId.HasValue && rootIds.Contains(c.ParentId.Value) && c.Status != CommentStatus.Deleted)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var rootDtos = _mapper.Map<List<CommentDto>>(rootComments);
        var replyDtos = _mapper.Map<List<CommentDto>>(replies);

        // Map replies into root comment tree
        foreach (var rootDto in rootDtos)
        {
            rootDto.Replies = replyDtos.Where(r => r.ParentId == rootDto.Id).ToList();
        }

        return new PagedResult<CommentDto>(rootDtos, totalCount, pageNumber, pageSize);
    }

    public async Task<CommentDto> CreateCommentAsync(long userId, long documentId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var docRepo = _unitOfWork.Repository<Document>();
        var doc = await docRepo.GetByIdAsync(documentId, cancellationToken);
        if (doc == null || doc.Status == DocumentStatus.Deleted)
        {
            throw new NotFoundException("Tài liệu", documentId);
        }

        if (request.ParentId.HasValue)
        {
            var parent = await _unitOfWork.Repository<Comment>().GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent == null || parent.DocumentId != documentId)
            {
                throw new NotFoundException("Bình luận cha", request.ParentId.Value);
            }
        }

        var comment = new Comment
        {
            DocumentId = documentId,
            UserId = userId,
            ParentId = request.ParentId,
            Content = request.Content.Trim(),
            Status = CommentStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var commentRepo = _unitOfWork.Repository<Comment>();
        await commentRepo.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
        var commenterName = user?.FullName ?? "Một người dùng";

        // Notify uploader if commenter is not uploader
        if (doc.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                doc.UserId,
                "Bình luận mới",
                $"{commenterName} đã bình luận về tài liệu '{doc.Title}'.",
                NotificationType.NewComment,
                $"/documents/{doc.Slug}",
                cancellationToken);
        }

        // Notify parent comment author if reply
        if (request.ParentId.HasValue)
        {
            var parentComment = await commentRepo.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parentComment != null && parentComment.UserId != userId && parentComment.UserId != doc.UserId)
            {
                await _notificationService.CreateNotificationAsync(
                    parentComment.UserId,
                    "Phản hồi bình luận",
                    $"{commenterName} đã trả lời bình luận của bạn trong tài liệu '{doc.Title}'.",
                    NotificationType.NewComment,
                    $"/documents/{doc.Slug}",
                    cancellationToken);
            }
        }

        var created = await commentRepo.Query().Include(c => c.User).FirstAsync(c => c.Id == comment.Id, cancellationToken);
        return _mapper.Map<CommentDto>(created);
    }

    public async Task<CommentDto> UpdateCommentAsync(long userId, long commentId, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Comment>();
        var comment = await repo.Query().Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        if (comment == null || comment.Status == CommentStatus.Deleted)
        {
            throw new NotFoundException("Bình luận", commentId);
        }

        if (comment.UserId != userId)
        {
            throw new ForbiddenException("Bạn không có quyền sửa bình luận này.");
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CommentDto>(comment);
    }

    public async Task DeleteCommentAsync(long userId, long commentId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Comment>();
        var comment = await repo.GetByIdAsync(commentId, cancellationToken);
        if (comment == null || comment.Status == CommentStatus.Deleted)
        {
            throw new NotFoundException("Bình luận", commentId);
        }

        if (comment.UserId != userId && !isAdmin)
        {
            throw new ForbiddenException("Bạn không có quyền xóa bình luận này.");
        }

        comment.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        repo.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
