using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Major : BaseEntity
{
    public long FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
