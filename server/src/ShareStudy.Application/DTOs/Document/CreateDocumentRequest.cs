using Microsoft.AspNetCore.Http;

namespace ShareStudy.Application.DTOs.Document;

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long EducationLevelId { get; set; }
    public long? SchoolId { get; set; }
    public long? FacultyId { get; set; }
    public long? MajorId { get; set; }
    public long? SubjectId { get; set; }
    public long DocumentTypeId { get; set; }
    public long? AcademicYearId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public IFormFile File { get; set; } = null!;
}

public class UpdateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long EducationLevelId { get; set; }
    public long? SchoolId { get; set; }
    public long? FacultyId { get; set; }
    public long? MajorId { get; set; }
    public long? SubjectId { get; set; }
    public long DocumentTypeId { get; set; }
    public long? AcademicYearId { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public IFormFile? File { get; set; }
}
