using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Authorize]
    public class RoundController : BaseController
    {
        private readonly IRoundService _roundService;

        public RoundController(IRoundService roundService)
        {
            _roundService = roundService;
        }

        [HttpGet("api/tracks/{trackId}/rounds")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetRoundsByTrackId(int trackId)
        {
            var result = await _roundService.GetRoundsByTrackIdAsync(trackId);
            return Ok(result);
        }

        [HttpGet("api/events/{eventId}/rounds")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetRoundsByEventId(int eventId)
        {
            var result = await _roundService.GetRoundsForSelectionByEventAsync(eventId);
            return Ok(result);
        }

        [HttpPost("api/rounds")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> CreateRound([FromBody] CreateRoundRequest request)
        {
            var result = await _roundService.CreateRoundAsync(request);
            return Ok(result);
        }

        [HttpPut("api/rounds/{id}")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> UpdateRound(int id, [FromBody] UpdateRoundRequest request)
        {
            var result = await _roundService.UpdateRoundAsync(id, request);
            return Ok(result);
        }

        [HttpPut("api/rounds/{id}/status")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> UpdateRoundStatus(int id, [FromBody] UpdateRoundStatusRequest request)
        {
            // Khi chuyển sang Active, service sẽ tự gán Topic cho các team đủ điều kiện.
            var result = await _roundService.UpdateRoundStatusAsync(id, request);
            return Ok(result);
        }

        [HttpPost("api/rounds/{id}/judges")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> AssignJudge(int id, [FromBody] AssignJudgeRequest request)
        {
            var assignedBy = GetCurrentAccountId();
            var result = await _roundService.AssignJudgeAsync(id, request, assignedBy);
            return Ok(result);
        }

        // [DEV 1 - API LẤY GIÁM KHẢO CỦA VÒNG THI]
        // Chức năng: Cung cấp endpoint cho FE lấy danh sách giám khảo để hiển thị trong màn hình setup Round.
        [HttpGet("api/rounds/{id}/judges")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetJudgesByRound(int id)
        {
            var result = await _roundService.GetJudgesByRoundAsync(id);
            return Ok(result);
        }

        [HttpGet("api/rounds/assigned")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> GetAssignedRounds()
        {
            var judgeId = GetCurrentAccountId();
            var result = await _roundService.GetAssignedRoundsForJudgeAsync(judgeId);
            return Ok(result);
        }
    }
}
