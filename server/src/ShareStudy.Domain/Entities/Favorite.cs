using ShareStudy.Domain.Common;

namespace ShareStudy.Domain.Entities;

public class Favorite : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;
}
