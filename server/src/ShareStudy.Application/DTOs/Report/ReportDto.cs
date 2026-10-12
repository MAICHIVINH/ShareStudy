namespace ShareStudy.Application.DTOs.Report;

public class ReportDto
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public long ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResolutionNote { get; set; }
    public long? ResolvedById { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateReportRequest
{
    public string Reason { get; set; } = string.Empty; // Copyright, WrongContent, BrokenFile, Spam, Inappropriate, Other
    public string? Description { get; set; }
}

public class ResolveReportRequest
{
    public string Status { get; set; } = string.Empty; // Resolved, Dismissed
    public string? ResolutionNote { get; set; }
}
