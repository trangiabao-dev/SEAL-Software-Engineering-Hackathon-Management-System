using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController  // kế thừa BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Ok(ApiResponse<object>.SuccessResult(
                null!, "Đăng ký thành công. Vui lòng chờ Coordinator duyệt tài khoản."
            ));
        }

        /// POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Đăng nhập thành công."));
        }

        /// POST api/auth/logout
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
        /// GET api/auth/pending
        /// Coordinator xem danh sách account chờ duyệt
        [HttpGet("pending")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> GetPendingAccounts()
        {
            var result = await _authService.GetPendingAccountsAsync();
            return Ok(ApiResponse<List<AccountPendingResponse>>.SuccessResult(result));
        }

        /// PUT api/auth/{id}/approve
        /// Coordinator duyệt account
        [HttpPut("{id:guid}/approve")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> ApproveAccount(Guid id)
        {
            var coordinatorId = GetCurrentAccountId();
            await _authService.ApproveAccountAsync(id, coordinatorId);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã duyệt tài khoản thành công."));
        }

        /// PUT api/auth/{id}/reject
        /// Coordinator từ chối account
        [HttpPut("{id:guid}/reject")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> RejectAccount(Guid id, [FromBody] RejectRequest request)
        {
            var coordinatorId = GetCurrentAccountId();
            await _authService.RejectAccountAsync(id, coordinatorId, request.Reason);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã từ chối tài khoản."));
        }
    }
}
