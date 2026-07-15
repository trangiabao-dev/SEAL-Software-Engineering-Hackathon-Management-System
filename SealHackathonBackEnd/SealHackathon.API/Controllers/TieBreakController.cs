using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.TieBreak;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller xử lý phiên chấm lại tie-break khi có đội đồng hạng.
    /// </summary>
    [ApiController]
    [Route("api/tie-breaks")]
    [Authorize]
    public class TieBreakController : BaseController
    {
        private readonly ITieBreakService _tieBreakService;

        /// <summary>
        /// Khởi tạo controller tie-break.
        /// </summary>
        public TieBreakController(ITieBreakService tieBreakService)
        {
            _tieBreakService = tieBreakService;
        }

        /// <summary>
        /// Coordinator tạo phiên tie-break cho một hạng đồng hạng trong Round.
        /// </summary>
        [HttpPost("rounds/{roundId:int}/rank/{rankPosition:int}")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> CreateSession(int roundId, int rankPosition)
        {
            var result = await _tieBreakService.CreateSessionAsync(roundId, rankPosition);

            return Ok(ApiResponse<TieBreakSessionResponse>.SuccessResult(
                result,
                "Tạo phiên tie-break thành công."));
        }

        /// <summary>
        /// Judge lấy danh sách phiên tie-break đang chờ mình chấm.
        /// </summary>
        [HttpGet("my-sessions")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> GetMyPendingSessions()
        {
            var judgeId = GetCurrentAccountId();
            var result = await _tieBreakService.GetMyPendingSessionsAsync(judgeId);

            return Ok(ApiResponse<List<TieBreakSessionResponse>>.SuccessResult(
                result,
                "Lấy danh sách phiên tie-break thành công."));
        }

        /// <summary>
        /// Coordinator lấy danh sách tất cả phiên tie-break của một Round.
        /// </summary>
        [HttpGet("rounds/{roundId:int}/sessions")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetSessionsByRound(int roundId)
        {
            var result = await _tieBreakService.GetSessionsByRoundAsync(roundId);

            return Ok(ApiResponse<List<TieBreakSessionResponse>>.SuccessResult(
                result,
                "Lấy danh sách phiên tie-break thành công."));
        }

        /// <summary>
        /// Lấy chi tiết một phiên tie-break.
        /// </summary>
        [HttpGet("{sessionId:guid}")]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge)]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            var currentAccountId = GetCurrentAccountId();
            var isCoordinator = User.IsInRole(RoleConstants.Coordinator);

            var result = await _tieBreakService.GetSessionAsync(
                sessionId,
                currentAccountId,
                isCoordinator);

            return Ok(ApiResponse<TieBreakSessionResponse>.SuccessResult(
                result,
                "Lấy chi tiết phiên tie-break thành công."));
        }

        /// <summary>
        /// Lấy danh sách điểm tie-break của một bài trong phiên chấm lại.
        /// </summary>
        [HttpGet("submissions/{tieBreakSubmissionId:guid}/scores")]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge)]
        public async Task<IActionResult> GetScoresByTieBreakSubmission(Guid tieBreakSubmissionId)
        {
            var currentAccountId = GetCurrentAccountId();
            var isCoordinator = User.IsInRole(RoleConstants.Coordinator);

            var result = await _tieBreakService.GetScoresByTieBreakSubmissionAsync(
                tieBreakSubmissionId,
                currentAccountId,
                isCoordinator);

            return Ok(ApiResponse<List<TieBreakScoreResponse>>.SuccessResult(
                result,
                "Lấy danh sách điểm tie-break thành công."));
        }

        /// <summary>
        /// Judge chấm một tiêu chí của một bài trong phiên tie-break.
        /// </summary>
        [HttpPost("submissions/{tieBreakSubmissionId:guid}/scores")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> SubmitScore(
            Guid tieBreakSubmissionId,
            [FromBody] SubmitTieBreakScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _tieBreakService.SubmitScoreAsync(
                tieBreakSubmissionId,
                judgeId,
                request);

            return Ok(ApiResponse<TieBreakScoreResponse>.SuccessResult(
                result,
                "Chấm điểm tie-break thành công."));
        }

        /// <summary>
        /// Judge chấm nhiều tiêu chí cùng lúc cho một bài trong phiên tie-break (Bulk Submit).
        /// </summary>
        [HttpPost("submissions/{tieBreakSubmissionId:guid}/scores/bulk")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> SubmitScores(
            Guid tieBreakSubmissionId,
            [FromBody] List<SubmitTieBreakScoreRequest> requests)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _tieBreakService.SubmitScoresAsync(
                tieBreakSubmissionId,
                judgeId,
                requests);

            return Ok(ApiResponse<List<TieBreakScoreResponse>>.SuccessResult(
                result,
                "Chấm nhiều điểm tie-break cùng lúc thành công."));
        }

        /// <summary>
        /// Judge sửa điểm tie-break do chính mình đã chấm.
        /// </summary>
        [HttpPut("scores/{tieBreakScoreRecordId:guid}")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> UpdateScore(
            Guid tieBreakScoreRecordId,
            [FromBody] UpdateTieBreakScoreRequest request)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _tieBreakService.UpdateScoreAsync(
                tieBreakScoreRecordId,
                judgeId,
                request);

            return Ok(ApiResponse<TieBreakScoreResponse>.SuccessResult(
                result,
                "Cập nhật điểm tie-break thành công."));
        }

        /// <summary>
        /// Coordinator tính kết quả tie-break sau khi Judge đã chấm đủ.
        /// </summary>
        [HttpPost("{sessionId:guid}/calculate-result")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> CalculateResult(Guid sessionId)
        {
            var result = await _tieBreakService.CalculateResultAsync(sessionId);

            return Ok(ApiResponse<TieBreakSessionResponse>.SuccessResult(
                result,
                "Tính kết quả tie-break thành công."));
        }
    }
}
