using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using System.Security.Claims;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/scores")]
    [Authorize] // Phải login mới được dùng — mọi API trong Controller này
    public class ScoreController : BaseController
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }
        

        // POST api/scores/submissions/{submissionId}
        // Chỉ Judge mới được chấm điểm
        [HttpPost("submissions/{submissionId}")]
        [Authorize(Roles = "Judge")]
        public async Task<IActionResult> SubmitScore(
            Guid submissionId,
            [FromBody] SubmitScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _scoreService.SubmitScoreAsync(submissionId, judgeId, request);
            return Ok(ApiResponse<ScoreRecordResponse>.SuccessResult(result, "Chấm điểm thành công."));
        }

        // GET api/scores/submissions/{submissionId}
        // Judge và Coordinator đều xem được
        [HttpGet("submissions/{submissionId}")]
        [Authorize(Roles = "Judge,Coordinator")]
        public async Task<IActionResult> GetScoresBySubmission(Guid submissionId)
        {
            var result = await _scoreService.GetScoresBySubmissionAsync(submissionId);
            return Ok(ApiResponse<List<ScoreRecordResponse>>.SuccessResult(result, "Lấy danh sách điểm thành công."));
        }
    }
    /*
    [Authorize] ở class — áp dụng cho toàn bộ Controller. Mọi API trong này đều cần login.
    [Authorize(Roles = "Judge")] ở method — ghi đè rule ở class, chỉ Judge mới chấm được.
    User.FindFirstValue(ClaimTypes.NameIdentifier) — đọc JudgeId từ JWT token. User là object built-in của ControllerBase, chứa thông tin người đang login.
    [FromBody] — báo .NET đọc dữ liệu từ request body (JSON), không phải từ URL.
    {submissionId} trong route — đây là route parameter, .NET tự map vào tham số Guid submissionId của hàm.
    */
}