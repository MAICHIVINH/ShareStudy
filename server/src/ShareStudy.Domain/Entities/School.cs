using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class School : BaseEntity
{
    public long EducationLevelId { get; set; }
    public EducationLevel EducationLevel { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
