using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.Ranking;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/rankings")]
    [Authorize] // Phải login mới được dùng
    public class RankingController : BaseController
    {
        private readonly IRankingService _rankingService;

        public RankingController(IRankingService rankingService)
        {
            _rankingService = rankingService;
        }

        // POST api/rankings/rounds/{roundId}/calculate
        // Chỉ Coordinator mới được trigger tính ranking
        /// <summary>
        /// Tính toán (hoặc tính lại) bảng xếp hạng cho 1 vòng thi.
        /// Xóa ranking cũ → tính lại từ ScoreRecord → lưu DB.
        /// </summary>
        [HttpPost("rounds/{roundId}/calculate")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> CalculateRanking(int roundId)
        {
            var result = await _rankingService.CalculateRankingAsync(roundId);
            return Ok(ApiResponse<RankingLeaderboardResponse>.SuccessResult(
                result, "Tính ranking thành công."));
        }

        // GET api/rankings/rounds/{roundId}
        // Coordinator và Judge đều xem được bảng xếp hạng
        /// <summary>
        /// Lấy bảng xếp hạng đã tính của 1 vòng thi.
        /// </summary>
        [HttpGet("rounds/{roundId}")]
        [Authorize(Roles = "Coordinator,Judge")]
        public async Task<IActionResult> GetLeaderboard(int roundId)
        {
            var result = await _rankingService.GetLeaderboardByRoundAsync(roundId);
            return Ok(ApiResponse<RankingLeaderboardResponse>.SuccessResult(
                result, "Lấy bảng xếp hạng thành công."));
        }

        // GET api/rankings/rounds/{roundId}/teams/{teamId}
        // Coordinator và Judge xem ranking của 1 team cụ thể
        /// <summary>
        /// Lấy ranking của 1 team trong 1 vòng thi.
        /// </summary>
        [HttpGet("rounds/{roundId}/teams/{teamId}")]
        [Authorize(Roles = "Coordinator,Judge")]
        public async Task<IActionResult> GetTeamRanking(int roundId, Guid teamId)
        {
            var result = await _rankingService.GetTeamRankingAsync(roundId, teamId);
            return Ok(ApiResponse<RankingResponse>.SuccessResult(
                result, "Lấy ranking của team thành công."));
        }
    }
}
