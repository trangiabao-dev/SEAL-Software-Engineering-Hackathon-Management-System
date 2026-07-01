using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Route("api/public/events")]
    [AllowAnonymous] // Không yêu cầu đăng nhập
    public class PublicEventController : BaseController
    {
        private readonly IEventService _eventService;

        public PublicEventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicEvents(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? status = null, 
            [FromQuery] string? search = null, 
            [FromQuery] string? sortBy = null)
        {
            var result = await _eventService.GetPublicEventsAsync(pageNumber, pageSize, status, search, sortBy);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicEventById(int id)
        {
            var result = await _eventService.GetPublicEventByIdAsync(id);
            return Ok(result);
        }
    }
}
