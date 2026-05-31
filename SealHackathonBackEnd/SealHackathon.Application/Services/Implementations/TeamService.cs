using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _uow;

        public TeamService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, Guid leaderId)
        {
            throw new NotImplementedException();
        }

        public async Task<TeamDetailDto> GetByIdAsync(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId)
        {
            throw new NotImplementedException();
        }
    }
}
