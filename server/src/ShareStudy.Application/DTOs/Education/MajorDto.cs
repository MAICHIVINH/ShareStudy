namespace ShareStudy.Application.DTOs.Education;

public class MajorDto
{
    public long Id { get; set; }
    public long FacultyId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public long SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SubjectCount { get; set; }
}

public class MajorDetailDto : MajorDto
{
    public List<SubjectDto> Subjects { get; set; } = new List<SubjectDto>();
}

public class CreateMajorRequest
{
    public long FacultyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateMajorRequest : CreateMajorRequest
{
}
