namespace ShareStudy.Application.DTOs.Document;

public class DocumentQueryParameters
{
    public string? Keyword { get; set; }
    public long? UserId { get; set; }
    public long? EducationLevelId { get; set; }
    public long? SchoolId { get; set; }
    public long? FacultyId { get; set; }
    public long? MajorId { get; set; }
    public long? SubjectId { get; set; }
    public long? DocumentTypeId { get; set; }
    public string? DocumentTypeName { get; set; }
    public long? AcademicYearId { get; set; }
    public string? FileType { get; set; }
    public double? MinRating { get; set; }
    public string? DateRange { get; set; } // today, 7days, 30days, 3months, 1year
    public string? Sort { get; set; } // newest, oldest, most-downloaded, highest-rated, most-viewed, title-asc, title-desc
    public string? SortBy { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
