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
                throw new ForbiddenException("Tài khoản của bạn không tồn tại hoặc đã bị vô hiệu hóa.");

            // Kiểm tra Track tồn tại
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", request.TrackId);

            // Kiểm tra Track còn chỗ
            if (track.MaxTeams is not null)
            {
                var teamCount = await _uow.GetRepository<Team>()
                    .CountAsync(t => t.TrackId == request.TrackId && !t.IsDeleted);

                if (teamCount >= track.MaxTeams)
                    throw new BadRequestException("Track này đã đạt số lượng đội tối đa.");
            }

            // Kiểm tra Leader đã có team trong Event này chưa
            var teamRepo = _uow.GetRepository<Team>();

            var existingTeam = await GetLeaderTeamInEventAsync(leaderId, track.EventId);
            if (existingTeam is not null)
                throw new ConflictException("Bạn đã có đội trong Event này.");

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

            // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
            if (team is null)
                throw new NotFoundException("Team", teamId);

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == teamId);

            return MapToDto(team, members);
        }

        public async Task<TeamDetailDto?> GetMyTeamAsync(Guid leaderId, int trackId)
        {
            if (trackId <= 0)
                throw new BadRequestException("TrackId không hợp lệ. TrackId phải lớn hơn 0.");

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", trackId);

            var team = await GetLeaderTeamInEventAsync(leaderId, track.EventId);

            if (team is null)
                return null;

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == team.Id);

            return MapToDto(team, members);
        }

        public async Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId)
        {
            var repo = _uow.GetRepository<Team>();

            // Tìm team
            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            // Kiểm tra quyền
            if (team.LeaderId != leaderId)
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa đội này.");

            // Kiểm tra nếu team đã được duyệt thì chỉ được phép cập nhật Link Github và không được đổi gì khác
            if (team.Status == TeamConstants.Status.Approved)
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
                        .GetFirstOrDefaultAsync(t => t.TeamName == request.TeamName
                                                  && t.Id != teamId
                                                  && !t.IsDeleted);

                    if (isNameTaken is not null)
                        throw new ConflictException("Tên đội này đã có người đăng ký. Vui lòng chọn tên khác.");

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

        public async Task<PaginatedResponse<TeamDetailDto>> GetAllTeamsAsync(
            int pageNumber, int pageSize, string? status, int? trackId)
        {
            if (pageNumber < 1)
                throw new BadRequestException("PageNumber phải lớn hơn hoặc bằng 1.");

            if (pageSize < 1 || pageSize > PaginationRequest.MaxPageSize)
                throw new BadRequestException($"PageSize phải nằm trong khoảng 1 đến {PaginationRequest.MaxPageSize}.");

            if (status is not null
                && status != TeamConstants.Status.Pending
                && status != TeamConstants.Status.Approved
                && status != TeamConstants.Status.Rejected
                && status != TeamConstants.Status.Disqualified)
            {
                throw new BadRequestException("Status không hợp lệ.");
            }

            Expression<Func<Team, bool>> predicate = t => !t.IsDeleted
                && (status == null || t.Status == status)
                && (trackId == null || t.TrackId == trackId);

            var totalRecords = await _uow.GetRepository<Team>().CountAsync(predicate);

            var skip = (pageNumber - 1) * pageSize;
            var teams = await _uow.GetRepository<Team>()
                .GetPagedAsync(predicate, skip, pageSize);

            var items = teams.Select(team => MapToDto(team)).ToList();

            return new PaginatedResponse<TeamDetailDto>(items, totalRecords, pageNumber, pageSize);
        }

        public async Task ApproveTeamAsync(Guid teamId, Guid coordinatorId)
        {
            var repo = _uow.GetRepository<Team>();

            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            if (team.Status != TeamConstants.Status.Pending)
                throw new BadRequestException("Chỉ có thể duyệt team đang ở trạng thái Pending.");

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

        public async Task DisqualifyTeamAsync(Guid teamId, Guid coordinatorId)
        {
            var repo = _uow.GetRepository<Team>();

            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException("Team này đã bị loại trước đó.");

            team.Status = TeamConstants.Status.Disqualified;
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = coordinatorId;

            await _uow.SaveChangesAsync();
        }

        public async Task AssignMentorAsync(Guid teamId, AssignMentorRequest request, Guid coordinatorId)
        {
            if (request.MentorId == Guid.Empty)
                throw new BadRequestException("MentorId không hợp lệ.");

            var teamRepo = _uow.GetRepository<Team>();

            var team = await teamRepo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null)
                throw new NotFoundException("Team", teamId);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == team.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", team.TrackId);

            var mentor = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == request.MentorId && !a.IsDeleted);

            if (mentor is null)
                throw new NotFoundException("Mentor", request.MentorId);

            var mentorEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == request.MentorId
                                           && ea.EventRole == RoleConstants.Mentor
                                           && ea.Status == "Approved");

            if (mentorEventRole is null)
                throw new BadRequestException("Tài khoản được chọn chưa được phân quyền Mentor trong Event này.");

            var mentorAssign = await _uow.GetRepository<MentorAssign>()
                .GetFirstOrDefaultAsync(ma => ma.MentorId == request.MentorId
                                           && ma.TrackId == team.TrackId);

            if (mentorAssign is null)
                throw new BadRequestException("Mentor này chưa được phân công vào Track của đội thi.");

            var currentTeamCount = await teamRepo.CountAsync(t => t.MentorId == request.MentorId
                                                               && t.TrackId == team.TrackId
                                                               && !t.IsDeleted);

            if (currentTeamCount >= TeamConstants.Rules.MaxTeamsPerMentor)
                throw new BadRequestException($"Mentor này đã hướng dẫn tối đa {TeamConstants.Rules.MaxTeamsPerMentor} đội. Vui lòng chọn Mentor khác.");

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
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException("Bạn không có quyền thêm thành viên vào đội này.");

            // Bước 2: Kiểm tra không quá 5 người
            var memberCount = await _uow.GetRepository<TeamMember>()
                .CountAsync(m => m.TeamId == teamId);

            if (memberCount >= TeamConstants.Rules.MaxMembersPerTeam)
                throw new BadRequestException($"Đội đã đạt giới hạn tối đa {TeamConstants.Rules.MaxMembersPerTeam} thành viên.");

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
            // Kiểm tra team tồn tại và quyền
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa thành viên của đội này.");

            // Tìm member
            var memberRepo = _uow.GetRepository<TeamMember>();
            var member = await memberRepo.GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);
            if (member is null)
                throw new NotFoundException("TeamMember", memberId);

            // Không cho sửa StudentCode — đây là định danh cố định
            // Chỉ cho sửa thông tin cá nhân
            member.FullName = request.FullName;
            member.Email = request.Email;
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
            if (team is null) // Không cho thao tác trên team đã bị xóa mềm hoặc không tồn tại.
                throw new NotFoundException("Team", teamId);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException("Bạn không có quyền xóa thành viên của đội này.");

            // Tìm member
            var memberRepo = _uow.GetRepository<TeamMember>();
            var member = await memberRepo.GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);
            if (member is null)
                throw new NotFoundException("TeamMember", memberId);

            // Không cho xóa Leader
            if (member.IsLeader)
                throw new BadRequestException("Không thể xóa Đội trưởng. Hãy chuyển quyền trước.");

            // Team đã duyệt phải luôn giữ tối thiểu 3 thành viên theo rule cuộc thi.
            if (team.Status == TeamConstants.Status.Approved)
            {
                var memberCount = await memberRepo.CountAsync(m => m.TeamId == teamId);
                if (memberCount <= 3)
                    throw new BadRequestException("Đội đã được duyệt. Cần ít nhất 3 thành viên. Không thể xóa thêm.");
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
                throw new NotFoundException("Track", trackId);

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
                throw new ConflictException($"StudentCode '{studentCode}' đã tồn tại trong một đội khác cùng Event.");
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
                Members = members?.Select(MapToMemberDto).ToList() ?? new List<TeamMemberDto>()
            };
        }

        private TeamMemberDto MapToMemberDto(TeamMember member)
        {
            return new TeamMemberDto
            {
                Id = member.Id,
                FullName = member.FullName,
                StudentCode = member.StudentCode,
                Email = member.Email,
                Phone = member.Phone,
                IsLeader = member.IsLeader,
                IsFPTStudent = member.IsFptstudent
            };
        }
    }
}