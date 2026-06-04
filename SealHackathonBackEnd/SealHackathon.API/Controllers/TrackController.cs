using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.DTOs.Track;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    // Đã gộp các Route như yêu cầu:
    // GET /api/events/:id/tracks (Lấy danh sách Tracks trong Event)
    // POST /api/tracks (Tạo Track mới)
    // PUT /api/tracks/:id (Cập nhật Track)
    
    [Authorize(Roles = "Coordinator")]
    public class TrackController : BaseController
    {
        private readonly ITrackService _trackService;

        public TrackController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        [HttpGet("api/events/{eventId}/tracks")]
        public async Task<IActionResult> GetTracksByEventId(int eventId)
        {
            var result = await _trackService.GetTracksByEventIdAsync(eventId);
            return Ok(result);
        }

        [HttpPost("api/tracks")]
        public async Task<IActionResult> CreateTrack([FromBody] CreateTrackRequest request)
        {
            var result = await _trackService.CreateTrackAsync(request);
            return Ok(result);
        }

        [HttpPut("api/tracks/{id}")]
        public async Task<IActionResult> UpdateTrack(int id, [FromBody] UpdateTrackRequest request)
        {
            var result = await _trackService.UpdateTrackAsync(id, request);
            return Ok(result);
        }

        [HttpPost("api/tracks/{id}/mentors")]
        public async Task<IActionResult> AssignMentor(int id, [FromBody] AssignMentorRequest request)
        {
            var assignedBy = GetCurrentAccountId();
            var result = await _trackService.AssignMentorAsync(id, request, assignedBy);
            return Ok(result);
        }
    }
}
