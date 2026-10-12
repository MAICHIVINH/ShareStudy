namespace ShareStudy.Application.DTOs.Education;

public class SubjectDto
{
    public long Id { get; set; }
    public long? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public long? MajorId { get; set; }
    public string? MajorName { get; set; }
    public long? FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public long? SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
}

public class CreateSubjectRequest
{
    public long? EducationLevelId { get; set; }
    public long? MajorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string? Description { get; set; }
}

public class UpdateSubjectRequest : CreateSubjectRequest
{
}

public class SubjectQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public long? MajorId { get; set; }
    public long? EducationLevelId { get; set; }
}
