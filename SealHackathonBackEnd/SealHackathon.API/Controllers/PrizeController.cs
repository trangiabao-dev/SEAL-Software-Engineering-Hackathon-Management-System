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
        /// Lấy danh sách cấu hình giải thưởng của một Event.
        /// </summary>
        [HttpGet("events/{eventId:int}")]
        public async Task<IActionResult> GetPrizesByEvent(int eventId)
        {
            var result = await _prizeService.GetPrizesByEventAsync(eventId);

            return Ok(ApiResponse<List<PrizeResponse>>.SuccessResult(
                result, "Lấy danh sách giải thưởng thành công."));
        }

        /// <summary>
        /// Tạo cấu hình giải thưởng cho một Event.
        /// </summary>
        [HttpPost("events/{eventId:int}")]
        public async Task<IActionResult> CreatePrize(int eventId, [FromBody] CreatePrizeRequest request)
        {
            var result = await _prizeService.CreatePrizeAsync(eventId, request);

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
        /// Lấy danh sách đội đạt giải hạng 1, 2, 3 của Event.
        /// </summary>
        [HttpGet("events/{eventId:int}/winners")]
        public async Task<IActionResult> GetWinnersByEvent(int eventId)
        {
            var result = await _prizeService.GetWinnersByEventAsync(eventId);

            return Ok(ApiResponse<List<PrizeWinnerResponse>>.SuccessResult(
                result, "Lấy danh sách đội đạt giải của Event thành công."));
        }

        /// <summary>
        /// Xuất danh sách đội đạt giải của Event ra file XLSX.
        /// </summary>
        [HttpGet("events/{eventId:int}/winners/export")]
        public async Task<IActionResult> ExportWinnersByEvent(int eventId)
        {
            var fileBytes = await _prizeService.ExportWinnersByEventAsync(eventId);
            var fileName = $"prize-winners-event-{eventId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

    }
}
