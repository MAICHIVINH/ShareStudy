namespace ShareStudy.Application.DTOs.Education;

public class EducationLevelDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SchoolCount { get; set; }
}

public class CreateEducationLevelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateEducationLevelRequest : CreateEducationLevelRequest
{
}
