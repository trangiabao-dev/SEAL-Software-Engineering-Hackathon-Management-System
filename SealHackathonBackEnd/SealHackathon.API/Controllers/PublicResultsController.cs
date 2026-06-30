using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.PublicResults;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller cung cấp dữ liệu công khai cho người xem không cần đăng nhập.
    /// </summary>
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicResultsController : ControllerBase
    {
        private readonly IPublicResultsService _publicResultsService;

        public PublicResultsController(IPublicResultsService publicResultsService)
        {
            _publicResultsService = publicResultsService;
        }

        /// <summary>
        /// Lấy kết quả công khai của một Event để hiển thị trên trang Results.
        /// </summary>
        [HttpGet("events/{eventId:int}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var result = await _publicResultsService.GetEventResultsAsync(eventId);

            return Ok(ApiResponse<PublicEventResultsResponse>.SuccessResult(
                result,
                "Lấy kết quả công khai của Event thành công."));
        }
    }
}
