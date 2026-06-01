using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
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
            // Bước 1: Kiểm tra Track tồn tại
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", request.TrackId);

            // Bước 2: Kiểm tra Track còn chỗ
            if (track.MaxTeams is not null)
            {
                var teamCount = await _uow.GetRepository<Team>()
                    .CountAsync(t => t.TrackId == request.TrackId && !t.IsDeleted);

                if (teamCount >= track.MaxTeams)
                    throw new BadRequestException("Track này đã đạt số lượng đội tối đa.");
            }

            // Bước 3: Kiểm tra Leader đã có team trong Track này chưa
            var teamRepo = _uow.GetRepository<Team>();

            var existingTeam = await teamRepo
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                          && t.TrackId == request.TrackId
                                          && !t.IsDeleted);

            if (existingTeam is not null)
                throw new ConflictException("Bạn đã có đội trong Track này.");

            // Tạo Team
            var newTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = request.TeamName,
                University = request.University,
                TrackId = request.TrackId,
                LeaderId = leaderId,
                GithubRepoLink = request.GithubRepoLink,
                Status = TeamStatus.Pending,  // ← constant
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = leaderId
            };

            await teamRepo.AddAsync(newTeam);

            // Bước 5: Tạo TeamMember cho Leader
            var leaderMember = new TeamMember
            {
                TeamId = newTeam.Id,
                FullName = request.FullName,
                StudentCode = request.StudentCode,
                Email = request.Email,
                Phone = request.Phone,
                IsLeader = true,
                IsFptstudent = request.IsFPTStudent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = leaderId
            };

            await _uow.GetRepository<TeamMember>().AddAsync(leaderMember);

            await _uow.SaveChangesAsync();

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

        public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId)
        {
            var repo = _uow.GetRepository<Team>();

            // Bước 1: Tìm team
            var team = await repo.GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null)
                throw new NotFoundException("Team", teamId);

            // Bước 2: Kiểm tra quyền
            if (team.LeaderId != leaderId)
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa đội này.");

            // Bước 3: Kiểm tra status
            if (team.Status == TeamStatus.Approved)  // ← constant
            {
                if (!string.Equals(team.TeamName, request.TeamName, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(team.University, request.University, StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException("Đội thi đã được duyệt. Bạn chỉ được phép cập nhật Link Github.");

                team.GithubRepoLink = request.GithubRepoLink;
            }
            else
            {
                if (!string.Equals(team.TeamName, request.TeamName, StringComparison.OrdinalIgnoreCase))
                {
                    var isNameTaken = await repo
                        .GetFirstOrDefaultAsync(t => t.TeamName.ToLower() == request.TeamName.ToLower()
                                                  && t.Id != teamId
                                                  && !t.IsDeleted);

                    if (isNameTaken is not null)
                        throw new ConflictException("Tên đội này đã có người đăng ký. Vui lòng chọn tên khác.");

                    team.TeamName = request.TeamName;
                }

                team.University = request.University;
                team.GithubRepoLink = request.GithubRepoLink;
            }

            // Bước 4: Lưu
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = leaderId;

            repo.Update(team);
            await _uow.SaveChangesAsync();

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
    }
}