using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Notification;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<NotificationResponse>>> GetMyNotificationsAsync(Guid accountId, int pageNumber = 1, int pageSize = 10)
        {
            var notifications = await _uow.GetRepository<Notification>()
                .GetAllAsync(n => n.AccountId == accountId);

            var pagedNotifications = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

            return ApiResponse<List<NotificationResponse>>.SuccessResult(pagedNotifications);
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid accountId)
        {
            var repo = _uow.GetRepository<Notification>();
            var notification = await repo.GetFirstOrDefaultTrackingAsync(n => n.Id == notificationId && n.AccountId == accountId);
            
            if (notification == null) throw new NotFoundException($"Không tìm thấy thông báo với ID {notificationId}");

            notification.IsRead = true;
            repo.Update(notification);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Đánh dấu đã đọc thành công.");
        }

        public async Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid accountId)
        {
            var repo = _uow.GetRepository<Notification>();
            // Note: Since GetAllAsync doesn't track, we use GetFirstOrDefaultTrackingAsync in a loop or direct EF Core update if we had raw access.
            // But with GenericRepository, we can just fetch all and update.
            var notifications = await repo.GetAllAsync(n => n.AccountId == accountId && !n.IsRead);
            
            foreach (var n in notifications)
            {
                var trackedN = await repo.GetFirstOrDefaultTrackingAsync(x => x.Id == n.Id);
                if (trackedN != null)
                {
                    trackedN.IsRead = true;
                    repo.Update(trackedN);
                }
            }
            
            if (notifications.Any())
            {
                await _uow.SaveChangesAsync();
            }

            return ApiResponse<bool>.SuccessResult(true, "Đã đánh dấu tất cả là đã đọc.");
        }

        public async Task SendNotificationAsync(CreateNotificationRequest request)
        {
            var notification = new Notification
            {
                AccountId = request.AccountId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GetRepository<Notification>().AddAsync(notification);
            await _uow.SaveChangesAsync();
        }
    }
}
