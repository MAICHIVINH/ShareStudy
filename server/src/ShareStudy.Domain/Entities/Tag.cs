using ShareStudy.Domain.Common;

namespace ShareStudy.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
}
