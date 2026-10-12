using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Notification;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(long userId, bool? isRead = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default);
    Task CreateNotificationAsync(long userId, string title, string message, NotificationType type, string? link = null, CancellationToken cancellationToken = default);
}
