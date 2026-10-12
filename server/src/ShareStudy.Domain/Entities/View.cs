using System.Net;
using ShareStudy.Domain.Common;

namespace ShareStudy.Domain.Entities;

public class View : BaseEntity
{
    public long? UserId { get; set; }
    public User? User { get; set; }

    public long DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public IPAddress? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset ViewedAt { get; set; } = DateTimeOffset.UtcNow;
}
