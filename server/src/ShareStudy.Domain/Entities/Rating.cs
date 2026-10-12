using ShareStudy.Domain.Common;

namespace ShareStudy.Domain.Entities;

public class Rating : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int Score { get; set; }
    public string? Comment { get; set; }
}
