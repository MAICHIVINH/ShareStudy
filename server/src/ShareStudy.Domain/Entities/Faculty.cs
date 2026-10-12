using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Faculty : BaseEntity
{
    public long SchoolId { get; set; }
    public School School { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Major> Majors { get; set; } = new List<Major>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
