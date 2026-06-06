using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using System.Security.Claims;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/scores")]
    [Authorize] // Ph?i login m?i du?c dùng — m?i API trong Controller này
    public class ScoreController : BaseController
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }
        

        // POST api/scores/submissions/{submissionId}
        // Ch? Judge m?i du?c ch?m di?m
        [HttpPost("submissions/{submissionId}")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> SubmitScore(
            Guid submissionId,
            [FromBody] SubmitScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _scoreService.SubmitScoreAsync(submissionId, judgeId, request);
            return Ok(ApiResponse<ScoreRecordResponse>.SuccessResult(result, "Ch?m di?m thành công."));
        }

        // GET api/scores/submissions/{submissionId}
        // Judge và Coordinator d?u xem du?c
        [HttpGet("submissions/{submissionId}")]
        [Authorize(Roles = RoleConstants.Judge + "," + RoleConstants.Coordinator)]
        public async Task<IActionResult> GetScoresBySubmission(Guid submissionId)
        {
            var result = await _scoreService.GetScoresBySubmissionAsync(submissionId);
            return Ok(ApiResponse<List<ScoreRecordResponse>>.SuccessResult(result, "L?y danh sách di?m thành công."));
        }
    }
    /*
    [Authorize] ? class — áp d?ng cho toàn b? Controller. M?i API trong này d?u c?n login.
    [Authorize(Roles = RoleConstants.Judge)] ? method — ghi dè rule ? class, ch? Judge m?i ch?m du?c.
    User.FindFirstValue(ClaimTypes.NameIdentifier) — d?c JudgeId t? JWT token. User là object built-in c?a ControllerBase, ch?a thông tin ngu?i dang login.
    [FromBody] — báo .NET d?c d? li?u t? request body (JSON), không ph?i t? URL.
    {submissionId} trong route — dây là route parameter, .NET t? map vào tham s? Guid submissionId c?a hàm.
    */
}