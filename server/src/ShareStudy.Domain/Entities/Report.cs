using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Report : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public ReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTimeOffset? ResolvedAt { get; set; }
    public long? ResolvedBy { get; set; }
    public User? ResolverUser { get; set; }
    public string? ResolutionNote { get; set; }
}
