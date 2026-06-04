using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;

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
        // GET api/auth/verify-email — Người dùng click vào link để xác nhận email
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

        // GET api/auth/pending — Coordinator xem danh sách chờ duyệt
        [HttpGet("pending")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> GetPendingAccounts()
        {
            var result = await _authService.GetPendingAccountsAsync();
            return Ok(ApiResponse<List<AccountPendingResponse>>.SuccessResult(result));
        }

        // PUT api/auth/{id}/approve — Coordinator duyệt
        [HttpPut("{id:guid}/approve")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> ApproveAccount(Guid id)
        {
            var coordinatorId = GetCurrentAccountId();
            await _authService.ApproveAccountAsync(id, coordinatorId);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã duyệt tài khoản thành công."));
        }

        // PUT api/auth/{id}/reject — Coordinator từ chối
        [HttpPut("{id:guid}/reject")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> RejectAccount(Guid id, [FromBody] RejectRequest request)
        {
            var coordinatorId = GetCurrentAccountId();
            await _authService.RejectAccountAsync(id, coordinatorId, request.Reason);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã từ chối tài khoản."));
        }
        // POST api/auth/create-staff — Coordinator tạo tài khoản cho Giám khảo / Mentor
        [HttpPost("create-staff")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> CreateStaffAccount([FromBody] CreateAccountRequest request)
        {
            // Lấy ID của Coordinator đang đăng nhập từ Token
            var coordinatorId = GetCurrentAccountId();

            await _authService.CreateAccountByCoordinatorAsync(request, coordinatorId);

            return Ok(ApiResponse<object>.SuccessResult(
                null!,
                $"Tạo tài khoản {request.Role} thành công. Mật khẩu tạm thời đã được gửi đến email {request.Email}."
            ));
        }
    }
}