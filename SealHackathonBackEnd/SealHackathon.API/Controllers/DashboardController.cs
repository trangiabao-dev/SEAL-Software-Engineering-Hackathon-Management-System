using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("coordinator")]
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> GetCoordinatorDashboard()
        {
            var result = await _dashboardService.GetCoordinatorDashboardAsync();
            return Ok(result);
        }
    }
}
