using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.Criteria;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [Route("api/rounds/{roundId:int}/criteria")]
    [Authorize]
    public class CriterionController : BaseController
    {
        private readonly ICriterionService _criterionService;

        public CriterionController(ICriterionService criterionService)
        {
            _criterionService = criterionService;
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Coordinator + "," + RoleConstants.Judge)]
        public async Task<IActionResult> GetCriteriaByRoundId(int roundId)
        {
            var result = await _criterionService.GetCriteriaByRoundIdAsync(roundId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCriterion(int roundId, [FromBody] CreateCriterionRequest request)
        {
            request.RoundId = roundId;
            var result = await _criterionService.CreateCriterionAsync(request);
            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCriterion(int roundId, [FromBody] ImportCriterionRequest request)
        {
            request.RoundId = roundId;
            var result = await _criterionService.ImportFromTemplateAsync(request);
            return Ok(result);
        }

        [HttpPut("{criterionId:int}")]
        public async Task<IActionResult> UpdateCriterion(
            int roundId, int criterionId, [FromBody] UpdateCriterionRequest request)
        {
            var result = await _criterionService.UpdateCriterionAsync(roundId, criterionId, request);
            return Ok(result);
        }

        [HttpDelete("{criterionId:int}")]
        public async Task<IActionResult> DeleteCriterion(int roundId, int criterionId)
        {
            var result = await _criterionService.DeleteCriterionAsync(roundId, criterionId);
            return Ok(result);
        }
    }
}
