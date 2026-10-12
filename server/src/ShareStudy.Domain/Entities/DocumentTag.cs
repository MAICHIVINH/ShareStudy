namespace ShareStudy.Domain.Entities;

public class DocumentTag
{
    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
