using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // [DEV 1 - LẤY DANH SÁCH THÔNG BÁO]
        // Chức năng: Cho phép User (bất kỳ role nào) lấy danh sách thông báo của họ.
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var accountId = GetCurrentAccountId();
            var result = await _notificationService.GetMyNotificationsAsync(accountId, pageNumber, pageSize);
            return Ok(result);
        }

        // [DEV 1 - ĐÁNH DẤU ĐÃ ĐỌC 1 THÔNG BÁO]
        // Chức năng: FE gọi API này khi User click vào 1 thông báo cụ thể.
        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var accountId = GetCurrentAccountId();
            var result = await _notificationService.MarkAsReadAsync(id, accountId);
            return Ok(result);
        }

        // [DEV 1 - ĐÁNH DẤU ĐÃ ĐỌC TẤT CẢ]
        // Chức năng: FE gọi API này khi User click "Đánh dấu tất cả là đã đọc".
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var accountId = GetCurrentAccountId();
            var result = await _notificationService.MarkAllAsReadAsync(accountId);
            return Ok(result);
        }
    }
}
