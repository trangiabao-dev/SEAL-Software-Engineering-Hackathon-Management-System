using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
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
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException("Team", teamId);

            return MapToDto(team);
        }
        private TeamDetailDto MapToDto(Team team)
        {
            return new TeamDetailDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                TrackId = team.TrackId,
                LeaderId = team.LeaderId,
                MentorId = team.MentorId,
                TopicId = team.TopicId,
                GithubRepoLink = team.GithubRepoLink,
                Status = team.Status
            };
        }

        public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId)
        {
            throw new NotImplementedException();
        }
    }
}
