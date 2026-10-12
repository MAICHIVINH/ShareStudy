using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Comment : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public long? ParentId { get; set; }
    public Comment? ParentComment { get; set; }

    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Active;

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
