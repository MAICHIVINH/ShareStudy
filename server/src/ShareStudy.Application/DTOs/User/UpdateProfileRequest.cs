namespace ShareStudy.Application.DTOs.User;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
}
