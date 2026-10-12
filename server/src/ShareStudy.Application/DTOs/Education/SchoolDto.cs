namespace ShareStudy.Application.DTOs.Education;

public class SchoolDto
{
    public long Id { get; set; }
    public long EducationLevelId { get; set; }
    public string EducationLevelName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int FacultyCount { get; set; }
    public int MajorCount { get; set; }
}

public class SchoolDetailDto : SchoolDto
{
    public List<FacultyDetailDto> Faculties { get; set; } = new List<FacultyDetailDto>();
}

public class CreateSchoolRequest
{
    public long EducationLevelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
}

public class UpdateSchoolRequest : CreateSchoolRequest
{
}
