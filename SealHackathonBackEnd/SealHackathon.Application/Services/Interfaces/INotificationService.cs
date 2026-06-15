using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationResponse>>> GetMyNotificationsAsync(Guid accountId, int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid accountId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid accountId);
        Task SendNotificationAsync(CreateNotificationRequest request);
    }
}
