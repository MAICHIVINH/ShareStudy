using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShareStudy.Application.Common;
using ShareStudy.Application.Common.Exceptions;
using ShareStudy.Application.DTOs.Notification;
using ShareStudy.Application.Interfaces;
using ShareStudy.Application.Interfaces.Repositories;
using ShareStudy.Domain.Entities;
using ShareStudy.Domain.Enums;

namespace ShareStudy.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(long userId, bool? isRead = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Repository<Notification>()
            .Query()
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            queryable = queryable.Where(n => n.IsRead == isRead.Value);
        }

        queryable = queryable.OrderByDescending(n => n.CreatedAt);

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<NotificationDto>>(items);
        return new PagedResult<NotificationDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<Notification>()
            .Query()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notif = await repo.GetByIdAsync(notificationId, cancellationToken);
        if (notif == null || notif.UserId != userId)
        {
            throw new NotFoundException("Thông báo", notificationId);
        }

        notif.IsRead = true;
        repo.Update(notif);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notif in unreadNotifications)
        {
            notif.IsRead = true;
            _unitOfWork.Repository<Notification>().Update(notif);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateNotificationAsync(long userId, string title, string message, NotificationType type, string? link = null, CancellationToken cancellationToken = default)
    {
        var notif = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = link,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
