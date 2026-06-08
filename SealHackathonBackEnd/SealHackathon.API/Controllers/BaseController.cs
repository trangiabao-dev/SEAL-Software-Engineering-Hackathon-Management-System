using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        //kiểm tra đủ 2 định dạng claim khác nhau, validate format GUID,
        //và throw lỗi rõ ràng nếu có gì sai — đảm bảo mọi code phía sau chỉ nhận được một Guid hợp lệ,
        //không bao giờ null hay sai format

        /// <summary>
        /// Khi FE gọi API, FE gửi token lên. Backend kiểm tra token đó có hợp lệ và đúng do hệ thống cấp ra không. 
        /// Nếu hợp lệ, backend đọc thông tin trong token và lấy được AccountId của người đang đăng nhập.
        /// </summary> 
        protected Guid GetCurrentAccountId()
        {
            // 1. Kiểm tra cả hai claim tiêu chuẩn NameIdentifier và "sub" phòng trường hợp cấu hình JWT khác nhau
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");

            // 2. Kiểm tra xem claim có bị rỗng hay không và có parse được sang GUID hợp lệ không
            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var accountId))
                throw new UnauthorizedAccessException("Không xác định được tài khoản.");

            return accountId;
        }
    }
}
