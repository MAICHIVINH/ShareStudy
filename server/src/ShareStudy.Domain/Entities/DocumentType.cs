using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class DocumentType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
