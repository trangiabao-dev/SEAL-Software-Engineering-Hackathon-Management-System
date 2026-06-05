using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Dashboard;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<CoordinatorDashboardResponse>> GetCoordinatorDashboardAsync();
    }
}
