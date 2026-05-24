using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.API.Controllers
{
    // [ApiController]: Bật các tính năng tự động cho API:
    // - Tự động trả 400 nếu request body không hợp lệ
    // - Tự động bind dữ liệu từ request vào parameter
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController  // kế thừa BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

    {
    }
}
