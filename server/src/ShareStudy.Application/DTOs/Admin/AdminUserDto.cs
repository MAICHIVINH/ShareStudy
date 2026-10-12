namespace ShareStudy.Application.DTOs.Admin;

public class AdminUserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}

public class UpdateUserRoleRequest
{
    public List<string> Roles { get; set; } = new List<string>();
}

public class RejectDocumentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public int TotalDocuments { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalViews { get; set; }
    public int PendingDocuments { get; set; }
    public int PendingReports { get; set; }
}

public class ChartDataItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class DashboardChartsDto
{
    public List<ChartDataItemDto> DocumentsByDay { get; set; } = new List<ChartDataItemDto>();
    public List<ChartDataItemDto> DownloadsByDay { get; set; } = new List<ChartDataItemDto>();
    public List<ChartDataItemDto> UsersByDay { get; set; } = new List<ChartDataItemDto>();
    public List<ChartDataItemDto> TopSubjects { get; set; } = new List<ChartDataItemDto>();
    public List<ChartDataItemDto> TopSchools { get; set; } = new List<ChartDataItemDto>();
}
