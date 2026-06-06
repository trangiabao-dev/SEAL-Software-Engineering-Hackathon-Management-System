using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.DTOs.CriterionTemplate;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Route("api/criterion-templates")]
    [Authorize(Roles = RoleConstants.Coordinator)]
    public class CriterionTemplateController : BaseController
    {
        private readonly ICriterionTemplateService _templateService;

        public CriterionTemplateController(ICriterionTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            var result = await _templateService.GetAllTemplatesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            var result = await _templateService.GetTemplateByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateCriterionTemplateRequest request)
        {
            var result = await _templateService.CreateTemplateAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var result = await _templateService.DeleteTemplateAsync(id);
            return Ok(result);
        }
    }
}
