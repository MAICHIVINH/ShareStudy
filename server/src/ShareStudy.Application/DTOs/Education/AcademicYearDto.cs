namespace ShareStudy.Application.DTOs.Education;

public class AcademicYearDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
}
