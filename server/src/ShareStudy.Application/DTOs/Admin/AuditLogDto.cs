namespace ShareStudy.Application.DTOs.Admin;

public class AuditLogDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Performer { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Severity { get; set; } = "INFO";
    public string Details { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
