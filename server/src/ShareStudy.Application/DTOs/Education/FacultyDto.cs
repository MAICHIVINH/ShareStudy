namespace ShareStudy.Application.DTOs.Education;

public class FacultyDto
{
    public long Id { get; set; }
    public long SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MajorCount { get; set; }
}

public class FacultyDetailDto : FacultyDto
{
    public List<MajorDto> Majors { get; set; } = new List<MajorDto>();
}

public class CreateFacultyRequest
{
    public long SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateFacultyRequest : CreateFacultyRequest
{
}
