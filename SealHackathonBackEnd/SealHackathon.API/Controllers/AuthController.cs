using Microsoft.AspNetCore.Mvc;

namespace SealHackathon.API.Controllers
{
    // [ApiController]: Bật các tính năng tự động cho API:
    // - Tự động trả 400 nếu request body không hợp lệ
    // - Tự động bind dữ liệu từ request vào parameter
    [ApiController]

    // [Route]: Định nghĩa URL prefix cho toàn bộ Controller này
    // "api/auth" → tất cả API trong này đều bắt đầu bằng /api/auth
    [Route("api/auth")]
    public class AuthController: Controller
    {
        // ControllerBase: class cha cung cấp các helper method
        // như Ok(), BadRequest(), NotFound(), Unauthorized()...
    }
}
