using SealHackathon.Application.Common.Requests;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

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
            // Kiểm tra Leader account còn active
            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(t => t.Id == leaderId && !t.IsDeleted);
            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            // Kiểm tra Track tồn tại
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // Kiểm tra Track còn chỗ
            if (track.MaxTeams is not null)
            {
                var teamCount = await _uow.GetRepository<Team>()
                    .CountAsync(t => t.TrackId == request.TrackId && !t.IsDeleted);

                if (teamCount >= track.MaxTeams)
                    throw new BadRequestException(ErrorMessages.Team.TrackFull);
            }

            // Kiểm tra Leader đã có team trong Event này chưa
            var teamRepo = _uow.GetRepository<Team>();

            var existingTeam = await GetLeaderTeamInEventAsync(leaderId, track.EventId);
            if (existingTeam is not null)
                throw new ConflictException(ErrorMessages.Team.AlreadyHasTeamInEvent);

            // Hàm này kiểm tra mã sinh viên chưa tồn tại trong bất kỳ team nào thuộc cùng Event.
            // Event -> Track -> Team -> TeamMember
            await CheckStudentCodeNotUsedInEventAsync(request.TrackId, request.StudentCode);

            // Team mới tạo luôn ở trạng thái Pending để Coordinator duyệt trước khi tham gia.
            var newTeam = new Team
            {
                Id = Guid.NewGuid(),
                TeamName = request.TeamName,
                University = request.University,
                TrackId = request.TrackId,
                LeaderId = leaderId,
                GithubRepoLink = request.GithubRepoLink,
                Status = TeamConstants.Status.Pending,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = leaderId
            };

            await teamRepo.AddAsync(newTeam); // Chuẩn bị insert entity này. Chưa lưu vào DB.

            // Tạo TeamMember cho Leader
            var leaderMember = new TeamMember
            {
                TeamId = newTeam.Id,
                FullName = request.FullName,
                StudentCode = request.StudentCode,
                Email = request.Email,
                University = request.University,
                Phone = request.Phone,
                IsLeader = true,
                IsFptstudent = request.IsFPTStudent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = leaderId
            };

            await _uow.GetRepository<TeamMember>().AddAsync(leaderMember); // Chuẩn bị insert entity này. Chưa lưu vào DB.

            await _uow.SaveChangesAsync();

            return MapToDto(newTeam, new List<TeamMember> { leaderMember }); // tạo team xong thấy luôn leader trong members
        }

        public async Task<TeamDetailDto> GetByIdAsync(Guid teamId)
        {
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == teamId);

            return MapToDto(team, members);
        }

        public async Task<TeamDetailDto?> GetMyTeamAsync(Guid leaderId, int eventId)
        {
            if (eventId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidEventId);

            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == leaderId && !a.IsDeleted);

            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                        && t.Track.EventId == eventId
                                        && !t.Track.IsDeleted
                                        && !t.IsDeleted);

            if (team is null)
                return null;

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == team.Id);

            return MapToDto(team, members);
        }

        public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId)
        {
            var repo = _uow.GetRepository<Team>();

            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            // Kiểm tra quyền
            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Team.NoUpdatePermission);

            // Kiểm tra nếu team đã được duyệt thì chỉ được phép cập nhật Link Github và không được đổi gì khác
            if (team.Status == TeamConstants.Status.Approved)
            {
                if (!string.Equals(team.TeamName, request.TeamName, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(team.University, request.University, StringComparison.OrdinalIgnoreCase))
                    throw new BadRequestException(ErrorMessages.Team.ApprovedOnlyGithubCanChange);

                team.GithubRepoLink = request.GithubRepoLink;
            }
            else
            {
                if (!string.Equals(team.TeamName, request.TeamName, StringComparison.OrdinalIgnoreCase))
                {
                    var isNameTaken = await repo
                        .GetFirstOrDefaultAsync(t => t.TeamName == request.TeamName
                                                  && t.Id != teamId
                                                  && !t.IsDeleted);

                    if (isNameTaken is not null)
                        throw new ConflictException(ErrorMessages.Team.NameAlreadyUsed);

                    team.TeamName = request.TeamName;
                }

                team.University = request.University;
                team.GithubRepoLink = request.GithubRepoLink;
            }

            // Lưu
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = leaderId;

            await _uow.SaveChangesAsync();

            var members = await _uow.GetRepository<TeamMember>().GetAllAsync(m => m.TeamId == team.Id);

            return MapToDto(team, members);
        }

        // =======================================================
        //                      COORDINATOR 
        // =======================================================

        public async Task<PaginatedResponse<TeamListDto>> GetAllTeamsAsync(
            int pageNumber, int pageSize, string? status, int? trackId)
        {
            if (pageNumber < 1)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageNumber);

            if (pageSize < 1 || pageSize > PaginationRequest.MaxPageSize)
                throw new BadRequestException($"PageSize phải nằm trong khoảng 1 đến {PaginationRequest.MaxPageSize}.");

            if (status is not null
                && status != TeamConstants.Status.Pending
                && status != TeamConstants.Status.Approved
                && status != TeamConstants.Status.Rejected
                && status != TeamConstants.Status.Disqualified)
            {
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);
            }

            Expression<Func<Team, bool>> predicate = t => !t.IsDeleted
                && (status == null || t.Status == status)
                && (trackId == null || t.TrackId == trackId);

            var totalRecords = await _uow.GetRepository<Team>().CountAsync(predicate);

            var skip = (pageNumber - 1) * pageSize;
            var teams = await _uow.GetRepository<Team>().GetPagedAsync(predicate, skip, pageSize);

            var teamIds = teams.Select(t => t.Id).ToList();

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => teamIds.Contains(m.TeamId));

            var memberCountByTeamId = members.GroupBy(m => m.TeamId)
                .ToDictionary(g => g.Key, g => g.Count());

            var items = teams
                .Select(team => MapToListDto(team, memberCountByTeamId.GetValueOrDefault(team.Id, 0)))
                .ToList();

            return new PaginatedResponse<TeamListDto>(items, totalRecords, pageNumber, pageSize);
        }

        public async Task ApproveTeamAsync(Guid teamId, Guid coordinatorId)
        {
            var repo = _uow.GetRepository<Team>();

            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.Status != TeamConstants.Status.Pending)
                throw new BadRequestException(ErrorMessages.Team.OnlyPendingCanApprove);

            // Kiểm tra team có đủ 3 thành viên không
            var memberCount = await _uow.GetRepository<TeamMember>()
                .CountAsync(m => m.TeamId == teamId);

            if (memberCount < TeamConstants.Rules.MinMembersPerTeam)
                throw new BadRequestException($"Đội thi phải có ít nhất {TeamConstants.Rules.MinMembersPerTeam} thành viên mới đủ điều kiện duyệt.");

            team.Status = TeamConstants.Status.Approved;
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = coordinatorId;

            await _uow.SaveChangesAsync();
        }

        public async Task DisqualifyTeamAsync(Guid teamId, DisqualifyTeamRequest request, Guid coordinatorId)
        {
            var teamRepo = _uow.GetRepository<Team>();

            var team = await teamRepo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

            var reason = request.Reason.Trim();
            var now = DateTime.UtcNow;

            team.Status = TeamConstants.Status.Disqualified;
            team.DisqualifyReason = reason;
            team.UpdatedAt = now;
            team.UpdatedBy = coordinatorId;

            var submissionRepo = _uow.GetRepository<Submission>();

            var submissions = await submissionRepo
                .GetAllAsync(s => s.TeamId == teamId && !s.IsDisqualified);

            foreach (var submission in submissions)
            {
                submission.IsDisqualified = true;
                submission.DisqualifyReason = reason;
                submission.DisqualifiedAt = now;
                submission.DisqualifiedBy = coordinatorId;

                submissionRepo.Update(submission);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task AssignMentorAsync(Guid teamId, AssignMentorRequest request, Guid coordinatorId)
        {
            if (request.MentorId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidMentorId);

            var teamRepo = _uow.GetRepository<Team>();

            var team = await teamRepo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == team.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var mentor = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == request.MentorId && !a.IsDeleted);

            if (mentor is null)
                throw new NotFoundException(ErrorMessages.Common.MentorNotFound);

            var mentorEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == request.MentorId
                                           && ea.EventRole == RoleConstants.Mentor
                                           && ea.Status == "Approved");

            if (mentorEventRole is null)
                throw new BadRequestException(ErrorMessages.Team.MentorNotInEvent);

            var mentorAssign = await _uow.GetRepository<MentorAssign>()
                .GetFirstOrDefaultAsync(ma => ma.MentorId == request.MentorId
                                           && ma.TrackId == team.TrackId);

            if (mentorAssign is null)
                throw new BadRequestException(ErrorMessages.Team.MentorNotAssignedToTrack);

            var currentTeamCount = await teamRepo.CountAsync(t => t.MentorId == request.MentorId
                                                               && t.TrackId == team.TrackId
                                                               && !t.IsDeleted);

            if (currentTeamCount >= TeamConstants.Rules.MaxTeamsPerMentor)
                throw new BadRequestException(ErrorMessages.Team.MentorMaxTeamsReached);

            team.MentorId = request.MentorId;
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = coordinatorId;

            await _uow.SaveChangesAsync();
        }

        // =======================================================
        //                    LEADER — MEMBER 
        // =======================================================
        public async Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddMemberRequest request, Guid leaderId)
        {
            var teamRepo = _uow.GetRepository<Team>();

            // Tìm team và kiểm tra quyền
            var team = await teamRepo.GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Team.NoAddMemberPermission);

            // Bước 2: Kiểm tra không quá 5 người
            var memberCount = await _uow.GetRepository<TeamMember>()
                .CountAsync(m => m.TeamId == teamId);

            if (memberCount >= TeamConstants.Rules.MaxMembersPerTeam)
                throw new BadRequestException(ErrorMessages.TeamMember.MaxMembersReached);

            // StudentCode không được trùng trong cùng Event.
            // Quan hệ DB: Event -> Track -> Team -> TeamMember.
            await CheckStudentCodeNotUsedInEventAsync(team.TrackId, request.StudentCode);

            // Tạo member mới
            var newMember = new TeamMember
            {
                TeamId = teamId,
                FullName = request.FullName,
                StudentCode = request.StudentCode,
                Email = request.Email,
                University = request.University,
                Phone = request.Phone,
                IsLeader = false,
                IsFptstudent = request.IsFPTStudent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = leaderId
            };

            await _uow.GetRepository<TeamMember>().AddAsync(newMember);
            await _uow.SaveChangesAsync();

            return MapToMemberDto(newMember);
        }

        public async Task<TeamMemberDto> UpdateMemberAsync(
            Guid teamId, int memberId, UpdateMemberRequest request, Guid leaderId)
        {
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Team.NoUpdateMemberPermission);

            // Tìm member
            var memberRepo = _uow.GetRepository<TeamMember>();

            var member = await memberRepo.GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);

            if (member is null)
                throw new NotFoundException(ErrorMessages.TeamMember.NotFound);

            // Không cho sửa StudentCode — đây là định danh cố định
            // Chỉ cho sửa thông tin cá nhân
            member.FullName = request.FullName;
            member.Email = request.Email;
            member.University = request.University;
            member.Phone = request.Phone;
            member.IsFptstudent = request.IsFPTStudent;
            member.UpdatedAt = DateTime.UtcNow;
            member.UpdatedBy = leaderId;

            await _uow.SaveChangesAsync();

            return MapToMemberDto(member);
        }

        public async Task DeleteMemberAsync(Guid teamId, int memberId, Guid leaderId)
        {
            // Kiểm tra team và quyền
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Team.NoDeleteMemberPermission);

            // Tìm member
            var memberRepo = _uow.GetRepository<TeamMember>();

            var member = await memberRepo.GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);
            if (member is null)
                throw new NotFoundException(ErrorMessages.TeamMember.NotFound);

            // Không cho xóa Leader
            if (member.IsLeader)
                throw new BadRequestException(ErrorMessages.TeamMember.CannotDeleteLeader);

            // Team đã duyệt phải luôn giữ tối thiểu 3 thành viên theo rule cuộc thi.
            if (team.Status == TeamConstants.Status.Approved)
            {
                var memberCount = await memberRepo.CountAsync(m => m.TeamId == teamId);
                if (memberCount <= 3)
                    throw new BadRequestException(ErrorMessages.TeamMember.ApprovedTeamMinMembersRequired);
            }


            // Xóa
            memberRepo.Delete(member);
            await _uow.SaveChangesAsync();
        }

        // =============== Business helpers ===============
        private async Task CheckStudentCodeNotUsedInEventAsync(int trackId, string studentCode)
        {
            var currentTrack = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (currentTrack is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var tracksInEvent = await _uow.GetRepository<Track>()
                .GetAllAsync(t => t.EventId == currentTrack.EventId && !t.IsDeleted);

            var trackIdsInEvent = tracksInEvent.Select(t => t.Id).ToList();

            var teamsInEvent = await _uow.GetRepository<Team>()
                .GetAllAsync(t => trackIdsInEvent.Contains(t.TrackId) && !t.IsDeleted);

            var teamIdsInEvent = teamsInEvent.Select(t => t.Id).ToList();

            var duplicateStudent = await _uow.GetRepository<TeamMember>()
                .GetFirstOrDefaultAsync(m => teamIdsInEvent.Contains(m.TeamId)
                                          && m.StudentCode == studentCode);

            if (duplicateStudent is not null)
                throw new ConflictException(ErrorMessages.TeamMember.StudentCodeAlreadyUsedInEvent);
        }

        private async Task<Team?> GetLeaderTeamInEventAsync(Guid leaderId, int eventId)
        {
            var tracksInEvent = await _uow.GetRepository<Track>()
                .GetAllAsync(t => t.EventId == eventId && !t.IsDeleted);

            var trackIdsInEvent = tracksInEvent.Select(t => t.Id).ToList();

            return await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                          && trackIdsInEvent.Contains(t.TrackId)
                                          && !t.IsDeleted);
        }

        // =============== Mapping helpers ===============
        private TeamDetailDto MapToDto(Team team, List<TeamMember>? members = null)
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
                Status = team.Status,
                // Nếu có members thì map từng member sang DTO.
                // Nếu members bị null thì trả về danh sách rỗng, không để FE nhận null.
                Members = members?.Select(MapToMemberDto).ToList() ?? new List<TeamMemberDto>()
                // Dấu ?. nghĩa là: nếu members khác null thì chạy tiếp; nếu members là null thì không gọi .Select(...).
                // Select(MapToMemberDto): Nghĩa là biến từng TeamMember entity thành TeamMemberDto.
                // .ToList(): Biến kết quả thành danh sách List<TeamMemberDto>.
                // ?? new List<TeamMemberDto>(): Nếu vế trái là null thì trả về list rỗng.
            };
        }

        // Hàm MapToMemberDto đổi từ dữ liệu DB ở bảng TeamMember -> sang dữ liệu trả FE là TeamMemberDto
        private TeamMemberDto MapToMemberDto(TeamMember member)
        {
            return new TeamMemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                StudentCode = member.StudentCode,
                Email = member.Email,
                University = member.University,
                Phone = member.Phone,
                IsLeader = member.IsLeader,
                IsFPTStudent = member.IsFptstudent
            };
        }

        /// <summary>
        /// Đếm số lượng thành viên của team để map vào TeamListDto trả về cho FE
        /// </summary>
        private TeamListDto MapToListDto(Team team, int memberCount)
        {
            return new TeamListDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                TrackId = team.TrackId,
                LeaderId = team.LeaderId,
                MentorId = team.MentorId,
                TopicId = team.TopicId,
                GithubRepoLink = team.GithubRepoLink,
                Status = team.Status,
                MemberCount = memberCount
            };
        }
    }
}
