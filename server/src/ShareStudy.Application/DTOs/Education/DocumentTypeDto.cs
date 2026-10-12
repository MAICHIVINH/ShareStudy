namespace ShareStudy.Application.DTOs.Education;

public class DocumentTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
}

public class CreateDocumentTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class UpdateDocumentTypeRequest : CreateDocumentTypeRequest
{
}
