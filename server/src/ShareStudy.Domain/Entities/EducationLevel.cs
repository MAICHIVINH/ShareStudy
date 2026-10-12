using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class EducationLevel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<School> Schools { get; set; } = new List<School>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
