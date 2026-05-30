using SealHackathon.Application.DTOs.Team;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, Guid leaderId);
        Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId);
        Task<TeamDetailDto> GetByIdAsync(Guid teamId);
    }
}