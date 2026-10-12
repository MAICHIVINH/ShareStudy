using ShareStudy.Domain.Common;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Domain.Entities;

public class Document : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long EducationLevelId { get; set; }
    public EducationLevel EducationLevel { get; set; } = null!;

    public long? SchoolId { get; set; }
    public School? School { get; set; }

    public long? FacultyId { get; set; }
    public Faculty? Faculty { get; set; }

    public long? MajorId { get; set; }
    public Major? Major { get; set; }

    public long? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public long DocumentTypeId { get; set; }
    public DocumentType DocumentType { get; set; } = null!;

    public long? AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public string? ThumbnailUrl { get; set; }

    public long ViewCount { get; set; } = 0;
    public long DownloadCount { get; set; } = 0;
    public long FavoriteCount { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public long RatingCount { get; set; } = 0;

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public DateTimeOffset? PublishedAt { get; set; }

    public ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<DownloadHistory> DownloadHistories { get; set; } = new List<DownloadHistory>();
    public ICollection<View> Views { get; set; } = new List<View>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
