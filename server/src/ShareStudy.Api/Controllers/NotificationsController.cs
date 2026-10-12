using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareStudy.Application.Common;
using ShareStudy.Application.DTOs.Notification;
using ShareStudy.Application.Interfaces;

namespace ShareStudy.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _notificationService.GetUserNotificationsAsync(userId, isRead, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        var result = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(ApiResponse<int>.SuccessResponse(result));
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(
        long id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        await _notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Đánh dấu đã đọc thành công"));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? 0;
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Đánh dấu tất cả đã đọc thành công"));
    }
}
