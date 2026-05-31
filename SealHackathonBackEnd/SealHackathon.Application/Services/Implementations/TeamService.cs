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
            // Kiểm tra Track tồn tại không
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", request.TrackId);

            // Kiểm tra Track còn chỗ không
            if (track.MaxTeams is not null)
            {
                var teamCount = await _uow.GetRepository<Team>()
                    .CountAsync(t => t.TrackId == request.TrackId && !t.IsDeleted);

                if (teamCount >= track.MaxTeams)
                    throw new BadRequestException("Track này đã đạt số lượng đội tối đa.");
            }

            // Kiểm tra Leader đã có team trong Track này chưa
            var teamRepo = _uow.GetRepository<Team>();

            var existingTeam = await teamRepo
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                          && t.TrackId == request.TrackId
                                          && !t.IsDeleted);

            if (existingTeam is not null)
                throw new ConflictException("Bạn đã có đội trong Track này.");

            // Tạo team mới
            var newTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = request.TeamName,
                University = request.University,
                TrackId = request.TrackId,
                LeaderId = leaderId,
                GithubRepoLink = request.GithubRepoLink,
                Status = "Pending",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow, // UtcNow: Luôn dùng giờ chuẩn quốc tế UTC
                UpdatedAt = DateTime.UtcNow, // Front-end sẽ hiển thị theo múi giờ người dùng
                CreatedBy = leaderId
            };

            // Lưu vào DB
            await teamRepo.AddAsync(newTeam);
            await _uow.SaveChangesAsync();

            // Trả về DTO
            return MapToDto(newTeam);
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
