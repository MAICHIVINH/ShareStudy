using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Subject : BaseEntity
{
    public long? EducationLevelId { get; set; }
    public EducationLevel? EducationLevel { get; set; }

    public long? MajorId { get; set; }
    public Major? Major { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
