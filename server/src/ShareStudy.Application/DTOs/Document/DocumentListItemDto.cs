namespace ShareStudy.Application.DTOs.Document;

public class DocumentListItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string EducationLevelName { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public string? FacultyName { get; set; }
    public string? MajorName { get; set; }
    public string? SubjectName { get; set; }
    public string CategorySummary { get; set; } = string.Empty;
    public long UploaderId { get; set; }
    public string UploaderName { get; set; } = string.Empty;
    public string? UploaderAvatar { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool IsFavorite { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DocumentDetailDto : DocumentListItemDto
{
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;
    public string? AcademicYearName { get; set; }
    public int FavoriteCount { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public string? PresignedPreviewUrl { get; set; }
}

public class DocumentDownloadDto
{
    public long DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

public class DocumentPreviewDto
{
    public long DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsSupported { get; set; }
    public string? PreviewUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}
