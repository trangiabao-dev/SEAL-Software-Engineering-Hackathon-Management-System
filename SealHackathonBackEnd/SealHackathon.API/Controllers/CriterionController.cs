using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.Criterion;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Authorize(Roles = RoleConstants.Coordinator)]
    public class CriterionController : BaseController
    {
        private readonly ICriterionService _criterionService;

        public CriterionController(ICriterionService criterionService)
        {
            _criterionService = criterionService;
        }

        [HttpGet("api/rounds/{roundId}/criteria")]
        public async Task<IActionResult> GetCriteriaByRoundId(int roundId)
        {
            var result = await _criterionService.GetCriteriaByRoundIdAsync(roundId);
            return Ok(result);
        }

        [HttpPost("api/rounds/{roundId}/criteria")]
        public async Task<IActionResult> CreateCriterion(int roundId, [FromBody] CreateCriterionRequest request)
        {
            request.RoundId = roundId;
            var result = await _criterionService.CreateCriterionAsync(request); // Rule 5: Check Weight sum
            return Ok(result);
        }

        [HttpPost("api/rounds/{roundId}/criteria/import")]
        public async Task<IActionResult> ImportCriterion(int roundId, [FromBody] ImportCriterionRequest request)
        {
            request.RoundId = roundId;
            var result = await _criterionService.ImportFromTemplateAsync(request);
            return Ok(result);
        }
    }
}
