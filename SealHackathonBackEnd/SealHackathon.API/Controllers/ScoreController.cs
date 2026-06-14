using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using System.Security.Claims;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller quản lý chấm điểm — yêu cầu login (JWT token)
    /// </summary>
    [ApiController]
    [Route("api/scores")]
    [Authorize]
    public class ScoreController : BaseController
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        /// <summary>
        /// Judge chấm điểm cho một Submission — POST api/scores/submissions/{submissionId}
        /// </summary>
        [HttpPost("submissions/{submissionId}")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> SubmitScore(
            Guid submissionId,
            [FromBody] SubmitScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _scoreService.SubmitScoreAsync(submissionId, judgeId, request);
            return Ok(ApiResponse<ScoreRecordResponse>.SuccessResult(result, "Chấm điểm thành công."));
        }

        /// <summary>
        /// Lấy danh sách điểm đã chấm của một Submission — GET api/scores/submissions/{submissionId}
        /// </summary>
        [HttpGet("submissions/{submissionId}")]
        [Authorize(Roles = RoleConstants.Judge + "," + RoleConstants.Coordinator)]
        public async Task<IActionResult> GetScoresBySubmission(Guid submissionId)
        {
            var currentAccountId = GetCurrentAccountId();
            var isCoordinator = User.IsInRole(RoleConstants.Coordinator);

            var result = await _scoreService.GetScoresBySubmissionAsync(
                submissionId, currentAccountId, isCoordinator);

            return Ok(ApiResponse<List<ScoreRecordResponse>>.SuccessResult(result, "Lấy danh sách điểm thành công."));
        }

        /// <summary>
        /// Judge sửa điểm đã chấm — PUT api/scores/{UpdateScoreRecordId}
        /// </summary>
        [HttpPut("{UpdateScoreRecordId}")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> UpdateScore(
            Guid UpdateScoreRecordId,
            [FromBody] UpdateScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _scoreService.UpdateScoreAsync(UpdateScoreRecordId, judgeId, request);
            return Ok(ApiResponse<ScoreRecordResponse>.SuccessResult(result, "Cập nhật điểm thành công."));
        }
    }
}