using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Criteria;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Criterion Service
    public interface ICriterionService
    {
        Task<ApiResponse<List<CriterionResponse>>> GetCriteriaByRoundIdAsync(int roundId);
        
        Task<ApiResponse<CriterionResponse>> CreateCriterionAsync(CreateCriterionRequest request); // Sẽ xử lý Rule 5 ở đây
        
        Task<ApiResponse<bool>> ImportFromTemplateAsync(ImportCriterionRequest request); // Logic import từ template

        Task<ApiResponse<CriterionResponse>> UpdateCriterionAsync(
            int roundId, int criterionId, UpdateCriterionRequest request);

        Task<ApiResponse<bool>> DeleteCriterionAsync(int roundId, int criterionId);
    }
}
