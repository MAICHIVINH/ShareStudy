using ShareStudy.Application.DTOs.Document;

namespace ShareStudy.Application.DTOs.Favorite;

public class FavoriteDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long DocumentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DocumentListItemDto Document { get; set; } = null!;
}
