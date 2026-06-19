using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.Topic;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Authorize(Roles = RoleConstants.Coordinator)]
    public class TopicController : BaseController
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        [HttpGet("api/rounds/{roundId}/topics")]
        public async Task<IActionResult> GetTopicsByRoundId(int roundId)
        {
            var result = await _topicService.GetTopicsByRoundIdAsync(roundId);
            return Ok(result);
        }

        [HttpPost("api/rounds/{roundId}/topics")]
        public async Task<IActionResult> CreateTopic(int roundId, [FromBody] CreateTopicRequest request)
        {
            request.RoundId = roundId; // Đảm bảo gán từ Route xuống DTO
            var result = await _topicService.CreateTopicAsync(request);
            return Ok(result);
        }

        [HttpPut("api/topics/{id}")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] UpdateTopicRequest request)
        {
            var result = await _topicService.UpdateTopicAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("api/topics/{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var result = await _topicService.DeleteTopicAsync(id);
            return Ok(result);
        }
    }
}
