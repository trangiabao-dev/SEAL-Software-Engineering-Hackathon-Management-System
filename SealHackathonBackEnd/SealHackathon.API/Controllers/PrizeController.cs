using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Prize;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller quản lý cấu hình giải thưởng và xuất kết quả đạt giải.
    /// </summary>
    [ApiController]
    [Route("api/prizes")]
    [Authorize(Roles = RoleConstants.Coordinator)]
    public class PrizeController : BaseController
    {
        private readonly IPrizeService _prizeService;

        public PrizeController(IPrizeService prizeService)
        {
            _prizeService = prizeService;
        }

        /// <summary>
        /// Lấy danh sách cấu hình giải thưởng của một Track.
        /// </summary>
        [HttpGet("tracks/{trackId:int}")]
        public async Task<IActionResult> GetPrizesByTrack(int trackId)
        {
            var result = await _prizeService.GetPrizesByTrackAsync(trackId);

            return Ok(ApiResponse<List<PrizeResponse>>.SuccessResult(
                result, "Lấy danh sách giải thưởng thành công."));
        }

        /// <summary>
        /// Tạo cấu hình giải thưởng cho một Track.
        /// </summary>
        [HttpPost("tracks/{trackId:int}")]
        public async Task<IActionResult> CreatePrize(int trackId, [FromBody] CreatePrizeRequest request)
        {
            var result = await _prizeService.CreatePrizeAsync(trackId, request);

            return Ok(ApiResponse<PrizeResponse>.SuccessResult(
                result, "Tạo giải thưởng thành công."));
        }

        /// <summary>
        /// Cập nhật cấu hình giải thưởng.
        /// </summary>
        [HttpPut("{prizeId:int}")]
        public async Task<IActionResult> UpdatePrize(int prizeId, [FromBody] UpdatePrizeRequest request)
        {
            var result = await _prizeService.UpdatePrizeAsync(prizeId, request);

            return Ok(ApiResponse<PrizeResponse>.SuccessResult(
                result, "Cập nhật giải thưởng thành công."));
        }

        /// <summary>
        /// Xóa cấu hình giải thưởng.
        /// </summary>
        [HttpDelete("{prizeId:int}")]
        public async Task<IActionResult> DeletePrize(int prizeId)
        {
            var result = await _prizeService.DeletePrizeAsync(prizeId);

            return Ok(ApiResponse<bool>.SuccessResult(
                result, "Xóa giải thưởng thành công."));
        }

        /// <summary>
        /// Lấy danh sách đội đạt giải hạng 1, 2, 3 của một Round.
        /// </summary>
        [HttpGet("rounds/{roundId:int}/winners")]
        public async Task<IActionResult> GetWinnersByRound(int roundId)
        {
            var result = await _prizeService.GetWinnersByRoundAsync(roundId);

            return Ok(ApiResponse<List<PrizeWinnerResponse>>.SuccessResult(
                result, "Lấy danh sách đội đạt giải thành công."));
        }

        /// <summary>
        /// Xuất danh sách đội đạt giải của một Round ra file XLSX.
        /// </summary>
        [HttpGet("rounds/{roundId:int}/winners/export")]
        public async Task<IActionResult> ExportWinnersByRound(int roundId)
        {
            var fileBytes = await _prizeService.ExportWinnersByRoundAsync(roundId);
            var fileName = $"prize-winners-round-{roundId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
