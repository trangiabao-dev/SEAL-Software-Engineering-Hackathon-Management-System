using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Ranking;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller quản lý xếp hạng (Ranking) — yêu cầu login (JWT token)
    /// </summary>
    [ApiController]
    [Route("api/rankings")]
    [Authorize]
    public class RankingController : BaseController
    {
        private readonly IRankingService _rankingService;

        public RankingController(IRankingService rankingService)
        {
            _rankingService = rankingService;
        }

        /// <summary>
        /// Coordinator tính toán (hoặc tính lại) bảng xếp hạng cho 1 vòng thi — xóa ranking cũ, tính lại từ ScoreRecord, lưu DB
        /// </summary>
        [HttpPost("rounds/{roundId}/calculate")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> CalculateRanking(int roundId)
        {
            var result = await _rankingService.CalculateRankingAsync(roundId);
            return Ok(ApiResponse<RankingLeaderboardResponse>.SuccessResult(
                result, "Tính ranking thành công."));
        }


        /// <summary>
        /// Lấy bảng xếp hạng đã tính của 1 vòng thi — đọc từ DB, không tính lại.
        /// </summary>
        [HttpGet("rounds/{roundId}")]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge + "," + RoleConstants.Leader)]
        public async Task<IActionResult> GetLeaderboard(int roundId)
        {
            var result = await _rankingService.GetLeaderboardByRoundAsync(roundId);
            return Ok(ApiResponse<RankingLeaderboardResponse>.SuccessResult(
                result, "Lấy bảng xếp hạng thành công."));
        }


        /// <summary>
        /// Lấy bảng xếp hạng chung cuộc của Event từ Final Round thuộc Track Final.
        /// </summary>
        [HttpGet("events/{eventId:int}")]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge + "," + RoleConstants.Leader)]
        public async Task<IActionResult> GetEventLeaderboard(int eventId)
        {
            var result = await _rankingService.GetLeaderboardByEventAsync(eventId);

            return Ok(ApiResponse<EventRankingResponse>.SuccessResult(
                result, "Lấy bảng xếp hạng Event thành công."));
        }

        /// <summary>
        /// Xuất bảng xếp hạng của một Round đã đóng ra file XLSX.
        /// </summary>
        [HttpGet("rounds/{roundId:int}/export")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> ExportRoundLeaderboard(int roundId)
        {
            var fileBytes = await _rankingService.ExportLeaderboardByRoundAsync(roundId);
            var fileName = $"ranking-round-{roundId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// Xuất bảng xếp hạng chung cuộc của Event ra file XLSX.
        /// </summary>
        [HttpGet("events/{eventId:int}/export")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> ExportEventLeaderboard(int eventId)
        {
            var fileBytes = await _rankingService.ExportLeaderboardByEventAsync(eventId);
            var fileName = $"ranking-event-{eventId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// Lấy ranking của 1 team cụ thể trong 1 vòng thi
        /// </summary>
        [HttpGet("rounds/{roundId}/teams/{teamId}")]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge + "," + RoleConstants.Leader)]
        public async Task<IActionResult> GetTeamRanking(int roundId, Guid teamId)
        {
            var result = await _rankingService.GetTeamRankingAsync(roundId, teamId);
            return Ok(ApiResponse<RankingResponse>.SuccessResult(
                result, "Lấy ranking của team thành công."));
        }
    }
}
