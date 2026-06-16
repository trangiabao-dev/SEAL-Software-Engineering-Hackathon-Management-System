using SealHackathon.Application.Common.Requests;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.DTOs.Topic;
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
        private readonly INotificationService _notificationService;

        public TeamService(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, Guid leaderId)
        {
            // Kiểm tra Leader account còn active
            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(t => t.Id == leaderId && !t.IsDeleted);
            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var eventOfTrack = await _uow.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == track.EventId && !e.IsDeleted);

            if (eventOfTrack is null)
                throw new NotFoundException("Không tìm thấy Event của Track.");

            if (!string.Equals(eventOfTrack.Status, EventConstants.Status.Registration, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Chỉ được tạo đội khi Event đang mở đăng ký.");

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

            var isNameTaken = await teamRepo
                .GetFirstOrDefaultAsync(t => t.TeamName == request.TeamName && !t.IsDeleted);

            if (isNameTaken is not null)
                throw new ConflictException(ErrorMessages.Team.NameAlreadyUsed);

            var existingTeam = await GetLeaderTeamInEventAsync(leaderId, track.EventId);
            if (existingTeam is not null)
                throw new ConflictException(ErrorMessages.Team.AlreadyHasTeamInEvent);

            // Kiểm tra mã sinh viên chưa tồn tại trong bất kỳ team nào thuộc cùng Event.
            await CheckStudentCodeNotUsedInEventAsync(track.EventId, request.StudentCode);

            await CheckEmailNotUsedInEventAsync(track.EventId, request.Email);

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

            var topic = await GetTopicForTeamAsync(team);

            return MapToDto(team, members, topic);
        }
        public async Task<TeamDetailDto?> GetMyTeamAsync(Guid leaderId)
        {
            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == leaderId && !a.IsDeleted);

            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            // Registration: Leader đang đăng ký / quản lý team trước khi thi.
            // Active: Event đang thi, Leader vẫn cần xem team của mình.
            var currentEvents = await _uow.GetRepository<Event>()
                .GetAllAsync(e => !e.IsDeleted
                               && (e.Status == EventConstants.Status.Registration
                                || e.Status == EventConstants.Status.Active));

            if (!currentEvents.Any())
                return null;

            if (currentEvents.Count > 1)
                throw new ConflictException("Hệ thống đang có nhiều hơn một Event đang mở hoặc đang diễn ra.");

            var currentEvent = currentEvents.First();

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                          && t.Track.EventId == currentEvent.Id
                                          && !t.Track.IsDeleted
                                          && !t.IsDeleted);

            if (team is null)
                return null;

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == team.Id);

            var topic = await GetTopicForTeamAsync(team);

            return MapToDto(team, members, topic);
        }

        public async Task<MyActiveRoundResponse?> GetMyActiveRoundAsync(Guid leaderId)
        {
            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == leaderId && !a.IsDeleted);

            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            var currentEvents = await _uow.GetRepository<Event>()
                .GetAllAsync(e => !e.IsDeleted && e.Status == EventConstants.Status.Active);

            if (!currentEvents.Any())
                return null;

            if (currentEvents.Count > 1)
                throw new ConflictException("Hệ thống đang có nhiều hơn một Event đang diễn ra.");

            var currentEvent = currentEvents.First();

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                            && t.Track.EventId == currentEvent.Id
                                            && !t.Track.IsDeleted && !t.IsDeleted);

            if (team is null)
                return null;

            if (team.Status != TeamConstants.Status.Approved)
                return null;

            var activeRounds = await _uow.GetRepository<Round>()
                .GetAllAsync(r => r.TrackId == team.TrackId && r.Status == RoundConstants.Status.Active);

            var activeRound = activeRounds.OrderBy(r => r.OrderIndex).FirstOrDefault();

            if (activeRound is null)
                return null;

            var roundTeam = await _uow.GetRepository<RoundTeam>()
                .GetFirstOrDefaultAsync(rt => rt.RoundId == activeRound.Id
                                           && rt.TeamId == team.Id
                                           && rt.TopicId != null);

            Topic? topic = null;

            if (roundTeam?.TopicId is not null)
            {
                topic = await _uow.GetRepository<Topic>()
                    .GetFirstOrDefaultAsync(t => t.Id == roundTeam.TopicId.Value);
            }

            return new MyActiveRoundResponse
            {
                RoundId = activeRound.Id,
                RoundName = activeRound.Name,
                Status = activeRound.Status,
                StartTime = activeRound.StartTime,
                EndTime = activeRound.EndTime,
                CanSubmit = topic is not null,
                Topic = MapToTopicDto(topic)
            };
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

            var topic = await GetTopicForTeamAsync(team);

            return MapToDto(team, members, topic);
        }

        // =======================================================
        //                      COORDINATOR 
        // =======================================================

        public async Task<PaginatedResponse<TeamListDto>> GetAllTeamsAsync(
            int pageNumber, int pageSize, string? status, int? trackId, int? eventId)
        {
            if (pageNumber < 1)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageNumber);

            if (pageSize < 1 || pageSize > PaginationRequest.MaxPageSize)
                throw new BadRequestException($"PageSize phải nằm trong khoảng 1 đến {PaginationRequest.MaxPageSize}.");

            // Nếu FE có gửi status, nhưng status đó không nằm trong danh sách status hợp lệ thì báo lỗi.
            if (status is not null && !TeamConstants.Status.ValidStatuses.Contains(status))
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);

            Expression<Func<Team, bool>> predicate = t => !t.IsDeleted
                && (status == null || t.Status == status)
                && (trackId == null || t.TrackId == trackId)
                && (eventId == null || t.Track.EventId == eventId);

            var totalRecords = await _uow.GetRepository<Team>().CountAsync(predicate);

            var skip = (pageNumber - 1) * pageSize;
            var teams = await _uow.GetRepository<Team>()
                .GetPagedAsync(predicate, t => t.CreatedAt, skip, pageSize, descending: true);

            var teamIds = teams.Select(t => t.Id).ToList();

            // Dictionary là kiểu dữ liệu lưu theo dạng: Key -> Value
            // Dictionary<Guid, int> : Key có kiểu Guid và Value có kiểu int
            var memberCountByTeamId = teamIds.Count == 0
                ? new Dictionary<Guid, int>() // TeamId -> số lượng member
                : await _uow.GetRepository<TeamMember>()
                    .CountByGroupAsync(m => teamIds.Contains(m.TeamId), m => m.TeamId);

            var items = teams
                .Select(team => MapToListDto(team, memberCountByTeamId.GetValueOrDefault(team.Id, 0)))
                .ToList();

            return new PaginatedResponse<TeamListDto>(items, totalRecords, pageNumber, pageSize);
        }

        public async Task<TeamGroupedByStatusDto> GetTeamsGroupedByStatusAsync(int eventId, int? trackId)
        {
            if (eventId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidEventId);

            Expression<Func<Team, bool>> predicate = t => !t.IsDeleted
                && t.Track.EventId == eventId
                && !t.Track.IsDeleted
                && (trackId == null || t.TrackId == trackId);

            var teams = await _uow.GetRepository<Team>().GetAllAsync(predicate);

            var teamIds = teams.Select(t => t.Id).ToList();

            var memberCountByTeamId = teamIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await _uow.GetRepository<TeamMember>()
                    .CountByGroupAsync(m => teamIds.Contains(m.TeamId), m => m.TeamId);

            var teamDtos = teams
                .Select(team => MapToListDto(team, memberCountByTeamId.GetValueOrDefault(team.Id, 0)))
                .ToList();

            var pending = teamDtos
                .Where(t => string.Equals(t.Status, TeamConstants.Status.Pending, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var approved = teamDtos
                .Where(t => string.Equals(t.Status, TeamConstants.Status.Approved, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var rejected = teamDtos
                .Where(t => string.Equals(t.Status, TeamConstants.Status.Rejected, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var disqualified = teamDtos
                .Where(t => string.Equals(t.Status, TeamConstants.Status.Disqualified, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new TeamGroupedByStatusDto
            {
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                Disqualified = disqualified,
                Counts = new TeamStatusCountDto
                {
                    Pending = pending.Count,
                    Approved = approved.Count,
                    Rejected = rejected.Count,
                    Disqualified = disqualified.Count,
                    Total = teamDtos.Count
                }
            };
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

            var oldTeamValues = new
            {
                team.Status,
                team.UpdatedAt,
                team.UpdatedBy
            };

            team.Status = TeamConstants.Status.Approved;
            team.UpdatedAt = DateTime.UtcNow;
            team.UpdatedBy = coordinatorId;

            await _uow.SaveChangesAsync();

            // Khi Coordinator bấm duyệt, hệ thống tự động đưa Notification "TEAM_APPROVED" cho Leader.
            await _notificationService.SendNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
            {
                AccountId = team.LeaderId,
                Title = "Đội thi đã được duyệt",
                Message = $"Đội thi {team.TeamName} của bạn đã được duyệt để tham gia sự kiện.",
                Type = "TEAM_APPROVED"
            });
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

            var oldTeamValues = new
            {
                team.Status,
                team.DisqualifyReason,
                team.UpdatedAt,
                team.UpdatedBy
            };

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

            var affectedSubmissionIds = submissions.Select(s => s.Id).ToList();

            await _uow.SaveChangesAsync();

            // Khi Coordinator bấm loại, hệ thống tự động đưa Notification "TEAM_DISQUALIFIED" kèm lý do cho Leader.
            await _notificationService.SendNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
            {
                AccountId = team.LeaderId,
                Title = "Đội thi đã bị loại",
                Message = $"Đội thi {team.TeamName} của bạn đã bị loại. Lý do: {reason}",
                Type = "TEAM_DISQUALIFIED"
            });
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
                                           && ea.Status == EventAccountConstants.Status.Approved);

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

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

            // Kiểm tra team không quá 5 người
            var memberCount = await _uow.GetRepository<TeamMember>()
                .CountAsync(m => m.TeamId == teamId);

            if (memberCount >= TeamConstants.Rules.MaxMembersPerTeam)
                throw new BadRequestException(ErrorMessages.TeamMember.MaxMembersReached);

            // StudentCode không được trùng trong cùng Event.
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == team.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            await CheckStudentCodeNotUsedInEventAsync(track.EventId, request.StudentCode);

            await CheckEmailNotUsedInEventAsync(track.EventId, request.Email);

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

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

            // Tìm member
            var memberRepo = _uow.GetRepository<TeamMember>();

            var member = await memberRepo
                .GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);

            if (member is null)
                throw new NotFoundException(ErrorMessages.TeamMember.NotFound);

            // Lấy Track của Team để biết Team này thuộc Event nào.
            // Sau đó dùng EventId để kiểm tra email không bị trùng trong cùng Event.
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == team.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // Khi update, bỏ qua chính member hiện tại để không tự báo trùng email của nó.
            await CheckEmailNotUsedInEventAsync(track.EventId, request.Email, member.Id);

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

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

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
            
            memberRepo.Delete(member);
            await _uow.SaveChangesAsync();
        }

        // =============== Business helpers ===============
        private async Task CheckStudentCodeNotUsedInEventAsync(int eventId, string studentCode)
        {
            var duplicateStudent = await _uow.GetRepository<TeamMember>()
                .GetFirstOrDefaultAsync(m => m.StudentCode == studentCode
                                          && !m.Team.IsDeleted
                                          && !m.Team.Track.IsDeleted
                                          && m.Team.Track.EventId == eventId);

            if (duplicateStudent is not null)
                throw new ConflictException(ErrorMessages.TeamMember.StudentCodeAlreadyUsedInEvent);
        }

        private async Task CheckEmailNotUsedInEventAsync(int eventId, string email, int? ignoreMemberId = null)
        {
            var duplicate = await _uow.GetRepository<TeamMember>()
                .GetFirstOrDefaultAsync(m => m.Email == email
                                          // HasValue - bool  → hỏi "cái này có chứa gì không?"
                                          // Value    - int   → hỏi "cái này đang chứa gì vậy?"
                                          && (!ignoreMemberId.HasValue || m.Id != ignoreMemberId.Value)
                                          && !m.Team.IsDeleted
                                          && !m.Team.Track.IsDeleted
                                          && m.Team.Track.EventId == eventId);

            if (duplicate is not null)
                throw new ConflictException(ErrorMessages.TeamMember.EmailAlreadyUsedInEvent);
        }

        private async Task<Team?> GetLeaderTeamInEventAsync(Guid leaderId, int eventId)
        {
            return await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                          && t.Track.EventId == eventId
                                          && !t.Track.IsDeleted
                                          && !t.IsDeleted);
        }

        private async Task<Topic?> GetTopicForTeamAsync(Team team)
        {
            // Leader chỉ được thấy đề khi hệ thống đã tạo RoundTeam.
            var roundTeams = await _uow.GetRepository<RoundTeam>()
                .GetAllAsync(rt => rt.TeamId == team.Id
                                && rt.TopicId != null
                                && rt.Round.Status != RoundConstants.Status.Upcoming);

            var latestRoundTeam = roundTeams
                .OrderByDescending(rt => rt.AssignedAt)
                .FirstOrDefault();

            if (latestRoundTeam?.TopicId is null)
                return null;

            return await _uow.GetRepository<Topic>()
                .GetFirstOrDefaultAsync(t => t.Id == latestRoundTeam.TopicId.Value);
        }

        // =============== Mapping helpers ===============
        private TeamDetailDto MapToDto(Team team, List<TeamMember>? members = null, Topic? topic = null)
        {
            return new TeamDetailDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                TrackId = team.TrackId,
                LeaderId = team.LeaderId,
                MentorId = team.MentorId,
                TopicId = topic?.Id,
                Topic = MapToTopicDto(topic),
                GithubRepoLink = team.GithubRepoLink,
                Status = team.Status,
                DisqualifyReason = team.DisqualifyReason,
                Members = members?.Select(MapToMemberDto).ToList() ?? new List<TeamMemberDto>()
            };
        }

        private static TopicResponse? MapToTopicDto(Topic? topic)
        {
            if (topic is null)
                return null;

            return new TopicResponse
            {
                Id = topic.Id,
                RoundId = topic.RoundId,
                Title = topic.Title,
                Description = topic.Description,
                Requirements = topic.Requirements,
                AttachmentUrl = topic.AttachmentUrl
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
                MemberCount = memberCount,
                DisqualifyReason = team.DisqualifyReason
            };
        }
    }
}
