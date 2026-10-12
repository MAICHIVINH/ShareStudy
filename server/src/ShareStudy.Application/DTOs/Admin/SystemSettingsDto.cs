namespace ShareStudy.Application.DTOs.Admin;

public class SystemSettingsDto
{
    public string SiteName { get; set; } = "ShareStudy - Hệ thống Chia sẻ Tài liệu Học tập";
    public string SiteSlogan { get; set; } = "Tri thức mở, kết nối cộng đồng học thuật Việt Nam";
    public string ContactEmail { get; set; } = "support@sharestudy.vn";
    public bool MaintenanceMode { get; set; } = false;

    // Security
    public bool AllowRegistration { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = true;
    public int MaxUploadMB { get; set; } = 50;
    public List<string> AllowedFileExtensions { get; set; } = new() { "pdf", "docx", "pptx", "xlsx", "zip" };

    // Notifications
    public bool NotifyOnDocumentApproval { get; set; } = true;
    public bool NotifyOnNewReport { get; set; } = true;
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpEmail { get; set; } = "no-reply@sharestudy.vn";

    // Storage
    public string StorageProvider { get; set; } = "Local";
    public string StorageBasePath { get; set; } = "./wwwroot/uploads";
}
