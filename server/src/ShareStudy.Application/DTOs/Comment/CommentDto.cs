namespace ShareStudy.Application.DTOs.Comment;

public class CommentDto
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public long? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
}

public class CreateCommentRequest
{
    public long? ParentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
