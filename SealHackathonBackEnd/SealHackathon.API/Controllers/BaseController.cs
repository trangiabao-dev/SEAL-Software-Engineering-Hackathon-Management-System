using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// Đọc AccountId của người đang login từ JWT claim.
        /// Dev 2 gọi GetCurrentAccountId() trong bất kỳ Controller nào kế thừa BaseController.
        /// </summary>
        protected Guid GetCurrentAccountId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var accountId))
                throw new UnauthorizedAccessException("Không xác định được tài khoản.");

            return accountId;
        }
    }
}
