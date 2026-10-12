namespace ShareStudy.Application.DTOs.Rating;

public class RatingDto
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateRatingRequest
{
    public int Score { get; set; }
    public string? Comment { get; set; }
}

public class UpdateRatingRequest
{
    public int Score { get; set; }
    public string? Comment { get; set; }
}
