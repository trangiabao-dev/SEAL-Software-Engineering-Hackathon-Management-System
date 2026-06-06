using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;


        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Ok(ApiResponse<object>.SuccessResult(
                null!, "Đăng ký thành công. Vui lòng xác nhận email."
            ));
        }
        // GET api/auth/verify-email — FE gọi API này (link email mở trang FE, FE gọi BE)
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            await _authService.VerifyEmailAsync(token);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Xác nhận email thành công. Bạn đã có thể đăng nhập."));
        }       
        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Đăng nhập thành công."));
        }

        // POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .ToString()["Bearer ".Length..]
                .Trim();

            await _authService.LogoutAsync(token);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đăng xuất thành công."));
        }

        // Bảo thêm cho Thức sửa lại rule Mentor và Judge - XÓA CreateStaffAccount
        // POST api/events/{eventId}/accounts/assign-role
        [HttpPost("/api/events/{eventId:int}/accounts/assign-role")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> AssignEventRole(int eventId, [FromBody] AssignEventRoleRequest request)
        {
            var coordinatorId = GetCurrentAccountId();

            await _authService.AssignEventRoleAsync(eventId, request, coordinatorId);

            return Ok(ApiResponse<object>.SuccessResult(
                null!,
                $"Đã phân quyền {request.EventRole} cho tài khoản trong sự kiện."
            ));
        }

        // Bảo thêm cho Thức sửa lại rule Mentor và Judge
        [HttpPost("create-guest-account")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> CreateGuestAccount([FromBody] CreateAccountRequest request)
        {
            var coordinatorId = GetCurrentAccountId();

            await _authService.CreateAccountByCoordinatorAsync(request, coordinatorId);

            return Ok(ApiResponse<object>.SuccessResult(
                null!,
                $"Tạo tài khoản khách mời {request.Role} thành công. Mật khẩu tạm thời đã được gửi đến email {request.Email}."
            ));
        }
    }
}