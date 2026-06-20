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
            // Leader phải còn hoạt động.
            await CheckLeaderAccountActiveAsync(leaderId);

            // Kiểm tra Leader và các thành viên không trùng email/mã sinh viên
            // ngay trong cùng request, khi dữ liệu chưa được lưu vào database.
            EnsureCreateTeamMembersAreUnique(request);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == request.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var eventOfTrack = await _uow.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == track.EventId && !e.IsDeleted);

            if (eventOfTrack is null)
                throw new NotFoundException("Không tìm thấy Event của Track.");

            if (!string.Equals(eventOfTrack.Status,
                EventConstants.Status.Registration, StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ được tạo đội khi Event đang mở đăng ký.");
            }

            // Không cho tạo thêm Team khi Track đã đủ số lượng.
            if (track.MaxTeams is not null)
            {
                var teamCount = await _uow.GetRepository<Team>()
                    .CountAsync(t => t.TrackId == request.TrackId && !t.IsDeleted);

                if (teamCount >= track.MaxTeams)
                    throw new BadRequestException(ErrorMessages.Team.TrackFull);
            }

            var teamRepo = _uow.GetRepository<Team>();

            var isNameTaken = await teamRepo.GetFirstOrDefaultAsync(
                t => t.TeamName == request.TeamName && !t.IsDeleted);

            if (isNameTaken is not null)
                throw new ConflictException(ErrorMessages.Team.NameAlreadyUsed);

            var existingTeam = await GetLeaderTeamInEventAsync(
                leaderId, track.EventId);

            if (existingTeam is not null)
                throw new ConflictException(ErrorMessages.Team.AlreadyHasTeamInEvent);

            // Kiểm tra Leader chưa xuất hiện trong Team khác của cùng Event.
            await CheckStudentCodeNotUsedInEventAsync(track.EventId, request.StudentCode);

            await CheckEmailNotUsedInEventAsync(track.EventId, request.Email);

            // Kiểm tra từng thành viên chưa xuất hiện trong Team khác của Event.
            // Phải kiểm tra hết trước khi chuẩn bị insert để tránh Team bị tạo dở.
            foreach (var memberRequest in request.Members)
            {
                await CheckStudentCodeNotUsedInEventAsync(track.EventId, memberRequest.StudentCode);

                await CheckEmailNotUsedInEventAsync(track.EventId, memberRequest.Email);
            }

            var now = DateTime.UtcNow;

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
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = leaderId
            };

            await teamRepo.AddAsync(newTeam);

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
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = leaderId
            };

            // Danh sách này dùng để vừa lưu database, vừa trả response đầy đủ.
            var teamMembers = new List<TeamMember>
            {
                leaderMember
            };

            foreach (var memberRequest in request.Members)
            {
                teamMembers.Add(new TeamMember
                {
                    TeamId = newTeam.Id,
                    FullName = memberRequest.FullName,
                    StudentCode = memberRequest.StudentCode,
                    Email = memberRequest.Email,
                    University = memberRequest.University,
                    Phone = memberRequest.Phone,
                    IsLeader = false,
                    IsFptstudent = memberRequest.IsFPTStudent,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = leaderId
                });
            }

            var memberRepo = _uow.GetRepository<TeamMember>();

            foreach (var teamMember in teamMembers)
            {
                await memberRepo.AddAsync(teamMember);
            }

            // Chỉ lưu một lần để Team và toàn bộ TeamMember cùng thành công
            // hoặc cùng thất bại.
            await _uow.SaveChangesAsync();

            return MapToDto(newTeam, teamMembers);
        }

        public async Task<TeamDetailDto> GetByIdAsync(
            Guid teamId, Guid currentAccountId, bool isCoordinator, bool isMentor)
        {
            if (teamId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidTeamId);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            var mentorCanViewTeam = false;

            if (isMentor && team.MentorId == currentAccountId)
            {
                var activeMentorRole = await _uow.GetRepository<EventAccount>()
                    .GetFirstOrDefaultAsync(eventAccount =>
                        eventAccount.AccountId == currentAccountId
                        && eventAccount.EventRole == RoleConstants.Mentor
                        && eventAccount.Status == EventAccountConstants.Status.Approved
                        && !eventAccount.Event.IsDeleted
                        && (eventAccount.Event.Status == EventConstants.Status.Registration
                            || eventAccount.Event.Status == EventConstants.Status.Active)
                        && eventAccount.Event.Tracks.Any(track =>
                            track.Id == team.TrackId && !track.IsDeleted));

                mentorCanViewTeam = activeMentorRole is not null;
            }

            var canViewTeam = isCoordinator || team.LeaderId == currentAccountId || mentorCanViewTeam;

            if (!canViewTeam)
                throw new ForbiddenException(ErrorMessages.Team.NoViewPermission);

            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(m => m.TeamId == teamId);

            var topic = await GetTopicForTeamAsync(team);

            return MapToDto(team, members, topic);
        }
        public async Task<TeamDetailDto?> GetMyTeamAsync(Guid leaderId)
        {
            await CheckLeaderAccountActiveAsync(leaderId);

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
            await CheckLeaderAccountActiveAsync(leaderId);

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
            await CheckLeaderAccountActiveAsync(leaderId);

            var repo = _uow.GetRepository<Team>();

            var team = await repo.GetFirstOrDefaultTrackingAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            // Kiểm tra quyền
            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Team.NoUpdatePermission);

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

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

        /// <summary>
        /// Lấy các Team thuộc Event hiện tại mà Mentor đang được phân công phụ trách.
        /// Thành viên được tải trong một truy vấn chung để tránh query riêng cho từng Team.
        /// </summary>
        public async Task<List<TeamDetailDto>> GetMyMentorTeamsAsync(Guid mentorId)
        {
            if (mentorId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidMentorId);

            var teams = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.MentorId == mentorId && !t.IsDeleted
                    && !t.Track.IsDeleted && !t.Track.Event.IsDeleted
                    && (t.Track.Event.Status == EventConstants.Status.Registration
                        || t.Track.Event.Status == EventConstants.Status.Active)
                    && t.Track.Event.EventAccounts.Any(eventAccount =>
                        eventAccount.AccountId == mentorId
                    && eventAccount.EventRole == RoleConstants.Mentor
                    && eventAccount.Status == EventAccountConstants.Status.Approved));

            if (teams.Count == 0)
                return new List<TeamDetailDto>();

            var teamIds = teams.Select(team => team.Id).ToList();

            // Lấy members của tất cả Team trong một lần thay vì query riêng từng Team.
            var members = await _uow.GetRepository<TeamMember>()
                .GetAllAsync(member => teamIds.Contains(member.TeamId));

            var membersByTeamId = members.GroupBy(member => member.TeamId)
                .ToDictionary(group => group.Key, group => group.ToList());

            return teams.OrderBy(team => team.TeamName)
                .Select(team => MapToDto(team, membersByTeamId
                .GetValueOrDefault(team.Id, new List<TeamMember>())))
                .ToList();
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

            // Chuẩn hóa status FE gửi lên để query luôn dùng đúng giá trị hệ thống đang lưu.
            var statusFilter = ResolveTeamStatusFilter(status);

            Expression<Func<Team, bool>> predicate = t => !t.IsDeleted
                && (statusFilter == null || t.Status == statusFilter)
                && (trackId == null || t.TrackId == trackId)
                && (eventId == null || t.Track.EventId == eventId);

            var totalRecords = await _uow.GetRepository<Team>().CountAsync(predicate);

            var skip = (pageNumber - 1) * pageSize;
            var teams = await _uow.GetRepository<Team>()
                .GetPagedAsync(predicate, t => t.CreatedAt, skip, pageSize, descending: true);

            var teamIds = teams.Select(t => t.Id).ToList();

            var leaderAccountsById = await GetLeaderAccountsAsync(teams);

            // Dictionary là kiểu dữ liệu lưu theo dạng: Key -> Value
            // Dictionary<Guid, int> : Key có kiểu Guid và Value có kiểu int
            var memberCountByTeamId = teamIds.Count == 0
                ? new Dictionary<Guid, int>() // TeamId -> số lượng member
                : await _uow.GetRepository<TeamMember>()
                    .CountByGroupAsync(m => teamIds.Contains(m.TeamId), m => m.TeamId);

            var items = teams.Select(team => MapToListDto(team,
                memberCountByTeamId.GetValueOrDefault(team.Id, 0),
                leaderAccountsById.GetValueOrDefault(team.LeaderId)))
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

            var leaderAccountsById = await GetLeaderAccountsAsync(teams);

            var teamIds = teams.Select(t => t.Id).ToList();

            var memberCountByTeamId = teamIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await _uow.GetRepository<TeamMember>()
                    .CountByGroupAsync(m => teamIds.Contains(m.TeamId), m => m.TeamId);

            var teamDtos = teams.Select(team => MapToListDto(team,
                memberCountByTeamId.GetValueOrDefault(team.Id, 0),
                leaderAccountsById.GetValueOrDefault(team.LeaderId)))
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
            var team = await GetTeamForMemberManagementAsync(
                teamId, leaderId, ErrorMessages.Team.NoAddMemberPermission);

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
            var team = await GetTeamForMemberManagementAsync(
                teamId, leaderId, ErrorMessages.Team.NoUpdateMemberPermission);

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
            var team = await GetTeamForMemberManagementAsync(
                teamId, leaderId, ErrorMessages.Team.NoDeleteMemberPermission);

            var memberRepo = _uow.GetRepository<TeamMember>();

            var member = await memberRepo
                .GetFirstOrDefaultTrackingAsync(m => m.Id == memberId && m.TeamId == teamId);

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
        /// <summary>
        /// Lấy team cho các thao tác quản lý thành viên và kiểm tra Leader có quyền thao tác.
        /// Cần helper này vì AddMember, UpdateMember và DeleteMember đều dùng chung rule:
        /// team phải tồn tại, Leader phải là chủ đội, và team chưa bị loại.
        /// </summary>
        private async Task<Team> GetTeamForMemberManagementAsync(Guid teamId, Guid leaderId, string forbiddenMessage)
        {
            await CheckLeaderAccountActiveAsync(leaderId);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException(forbiddenMessage);

            if (team.Status == TeamConstants.Status.Disqualified)
                throw new BadRequestException(ErrorMessages.Team.AlreadyDisqualified);

            return team;
        }

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
                                          // Khi update member, bỏ qua chính member hiện tại để không tự báo trùng email.
                                          && (!ignoreMemberId.HasValue || m.Id != ignoreMemberId.Value)
                                          && !m.Team.IsDeleted
                                          && !m.Team.Track.IsDeleted
                                          && m.Team.Track.EventId == eventId);

            if (duplicate is not null)
                throw new ConflictException(ErrorMessages.TeamMember.EmailAlreadyUsedInEvent);
        }

        /// <summary>
        /// Tập trung logic kiểm tra Leader còn tồn tại và chưa bị xóa mềm.
        /// Cần helper này vì nhiều hàm Leader đều phải chặn account đã bị deactivate nhưng token vẫn còn hạn.
        /// </summary>
        private async Task CheckLeaderAccountActiveAsync(Guid leaderId)
        {
            if (leaderId == Guid.Empty)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);

            var leaderAccount = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == leaderId && !a.IsDeleted);

            if (leaderAccount is null)
                throw new ForbiddenException(ErrorMessages.Common.InvalidAccount);
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

        /// <summary>
        /// Chuẩn hóa status team FE gửi lên thành đúng format BE đang lưu.
        /// Cần helper này để tránh lặp logic validate status và tránh lỗi khác hoa/thường khi FE gửi query.
        /// </summary>
        private static string? ResolveTeamStatusFilter(string? status)
        {
            if (status is null)
                return null;

            status = status.Trim();

            if (string.IsNullOrWhiteSpace(status))
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);

            if (string.Equals(status, TeamConstants.Status.Pending, StringComparison.OrdinalIgnoreCase))
                return TeamConstants.Status.Pending;

            if (string.Equals(status, TeamConstants.Status.Approved, StringComparison.OrdinalIgnoreCase))
                return TeamConstants.Status.Approved;

            if (string.Equals(status, TeamConstants.Status.Rejected, StringComparison.OrdinalIgnoreCase))
                return TeamConstants.Status.Rejected;

            if (string.Equals(status, TeamConstants.Status.Disqualified, StringComparison.OrdinalIgnoreCase))
                return TeamConstants.Status.Disqualified;

            throw new BadRequestException(ErrorMessages.Common.InvalidStatus);
        }

        // =============== Private helpers ===============
        // Kiểm tra Leader và các thành viên gửi trong cùng request không trùng định danh.
        // Cần kiểm tra trên RAM vì những thành viên này chưa tồn tại trong database.
        private static void EnsureCreateTeamMembersAreUnique(CreateTeamRequest request)
        {
            var studentCodes = request.Members
                .Select(member => NormalizeStudentCode(member.StudentCode))
                .Append(NormalizeStudentCode(request.StudentCode))
                .ToList();

            if (studentCodes.Distinct().Count() != studentCodes.Count)
            {
                throw new ConflictException(
                    ErrorMessages.TeamMember.DuplicateStudentCodeInRequest);
            }

            var emails = request.Members
                .Select(member => NormalizeEmail(member.Email))
                .Append(NormalizeEmail(request.Email))
                .ToList();

            if (emails.Distinct().Count() != emails.Count)
            {
                throw new ConflictException(
                    ErrorMessages.TeamMember.DuplicateEmailInRequest);
            }
        }

        // Chuyển mã SV "se190374" và " SE190374 "
        private static string NormalizeStudentCode(string studentCode)
        {
            return studentCode.Trim().ToUpperInvariant();
        }

        // Chuẩn hóa email để khác chữ hoa/thường hoặc khoảng trắng vẫn được xem là trùng.
        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Lấy tài khoản Leader của nhiều Team trong một truy vấn để tránh query riêng từng Team.
        /// </summary>
        private async Task<Dictionary<Guid, Account>> GetLeaderAccountsAsync(List<Team> teams)
        {
            var leaderIds = teams.Select(team => team.LeaderId).Distinct().ToList();

            if (leaderIds.Count == 0)
                return new Dictionary<Guid, Account>();

            var leaderAccounts = await _uow.GetRepository<Account>()
                .GetAllAsync(account => leaderIds.Contains(account.Id));

            return leaderAccounts.ToDictionary(account => account.Id, account => account);
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
        /// Chuyển Team thành dữ liệu tóm tắt và bổ sung thông tin tài khoản Leader.
        /// </summary>
        private TeamListDto MapToListDto(Team team, int memberCount, Account? leaderAccount)
        {
            return new TeamListDto
            {
                Id = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                TrackId = team.TrackId,
                LeaderId = team.LeaderId,
                LeaderUsername = leaderAccount?.Username,
                LeaderEmail = leaderAccount?.Email,
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
