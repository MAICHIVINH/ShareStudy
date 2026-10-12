using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
