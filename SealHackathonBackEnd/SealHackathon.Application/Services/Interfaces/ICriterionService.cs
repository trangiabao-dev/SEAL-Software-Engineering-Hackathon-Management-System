using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Criterion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Criterion Service
    public interface ICriterionService
    {
        Task<ApiResponse<List<CriterionResponse>>> GetCriteriaByRoundIdAsync(int roundId);
        Task<ApiResponse<CriterionResponse>> CreateCriterionAsync(CreateCriterionRequest request); // Sẽ xử lý Rule 5 ở đây
        Task<ApiResponse<bool>> ImportFromTemplateAsync(ImportCriterionRequest request); // Logic import từ template
    }
}
