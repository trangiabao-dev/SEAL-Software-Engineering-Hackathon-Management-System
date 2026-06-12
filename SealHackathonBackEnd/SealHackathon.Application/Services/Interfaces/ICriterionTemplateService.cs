using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.CriterionTemplate;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface ICriterionTemplateService
    {
        Task<ApiResponse<List<CriterionTemplateResponse>>> GetAllTemplatesAsync();
        Task<ApiResponse<CriterionTemplateResponse>> GetTemplateByIdAsync(int id);
        Task<ApiResponse<CriterionTemplateResponse>> CreateTemplateAsync(CreateCriterionTemplateRequest request);
        Task<ApiResponse<CriterionTemplateResponse>> UpdateTemplateAsync(int id, UpdateCriterionTemplateRequest request);
        Task<ApiResponse<bool>> DeleteTemplateAsync(int id);
    }
}
