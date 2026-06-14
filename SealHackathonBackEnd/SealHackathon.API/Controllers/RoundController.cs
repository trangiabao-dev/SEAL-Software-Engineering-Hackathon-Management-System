using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Authorize(Roles = RoleConstants.Coordinator)]
    public class RoundController : BaseController
    {
        private readonly IRoundService _roundService;

        public RoundController(IRoundService roundService)
        {
            _roundService = roundService;
        }

        [HttpGet("api/tracks/{trackId}/rounds")]
        public async Task<IActionResult> GetRoundsByTrackId(int trackId)
        {
            var result = await _roundService.GetRoundsByTrackIdAsync(trackId);
            return Ok(result);
        }

        [HttpPost("api/rounds")]
        public async Task<IActionResult> CreateRound([FromBody] CreateRoundRequest request)
        {
            var result = await _roundService.CreateRoundAsync(request);
            return Ok(result);
        }

        [HttpPut("api/rounds/{id}")]
        public async Task<IActionResult> UpdateRound(int id, [FromBody] UpdateRoundRequest request)
        {
            var result = await _roundService.UpdateRoundAsync(id, request);
            return Ok(result);
        }

        [HttpPut("api/rounds/{id}/status")]
        public async Task<IActionResult> UpdateRoundStatus(int id, [FromBody] UpdateRoundStatusRequest request)
        {
            // Khi chuyển sang Active, service sẽ tự gán Topic cho các team đủ điều kiện.
            var result = await _roundService.UpdateRoundStatusAsync(id, request);
            return Ok(result);
        }

        [HttpPost("api/rounds/{id}/judges")]
        public async Task<IActionResult> AssignJudge(int id, [FromBody] AssignJudgeRequest request)
        {
            var assignedBy = GetCurrentAccountId();
            var result = await _roundService.AssignJudgeAsync(id, request, assignedBy);
            return Ok(result);
        }
    }
}
