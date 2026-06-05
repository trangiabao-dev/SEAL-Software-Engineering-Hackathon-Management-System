using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.CriterionTemplate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface ICriterionTemplateService
    {
        Task<ApiResponse<List<CriterionTemplateResponse>>> GetAllTemplatesAsync();
        Task<ApiResponse<CriterionTemplateResponse>> GetTemplateByIdAsync(int id);
        Task<ApiResponse<CriterionTemplateResponse>> CreateTemplateAsync(CreateCriterionTemplateRequest request);
        Task<ApiResponse<bool>> DeleteTemplateAsync(int id);
    }
}
