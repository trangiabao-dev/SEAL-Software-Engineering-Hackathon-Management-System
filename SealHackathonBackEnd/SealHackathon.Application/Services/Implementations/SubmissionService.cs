using SealHackathon.Application.DTOs.Submission;
using SealHackathon.Application.DTOs.Batch;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _uow;

        public SubmissionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<SubmissionDto> CreateSubmissionAsync(int roundId,
            CreateSubmissionRequest request, Guid leaderId)
        {
            if (roundId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidRoundId);

            ValidatePresentationUrl(request.PresentationUrl);

            var round = await GetRoundOrThrowAsync(roundId);
            ValidateRoundAcceptsSubmissions(round, ErrorMessages.Submission.DeadlinePassed);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.LeaderId == leaderId
                                            && t.TrackId == round.TrackId
                                            && !t.IsDeleted);

            if (team is null)
                throw new ForbiddenException(ErrorMessages.Submission.NoTeamInRoundTrack);

            if (team.Status != TeamConstants.Status.Approved)
                throw new BadRequestException(ErrorMessages.Submission.TeamNotApproved);

            if (string.IsNullOrWhiteSpace(team.GithubRepoLink))
                throw new BadRequestException(ErrorMessages.Submission.TeamGithubRepoRequired);

            await EnsureTeamCanSubmitRoundAsync(team.Id, round.Id);

            var existingSubmission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.TeamId == team.Id && s.RoundId == round.Id);

            if (existingSubmission is not null)
                throw new ConflictException(ErrorMessages.Submission.AlreadySubmitted);

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                RoundId = round.Id,
                PresentationUrl = request.PresentationUrl,
                IsDisqualified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GetRepository<Submission>().AddAsync(submission);

            await _uow.SaveChangesAsync();

            return MapToDto(submission);
        }

        public async Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId,
            UpdateSubmissionRequest request, Guid leaderId)
        {
            // Chặn SubmissionId rỗng để trả lỗi request sai thay vì đi xuống database.
            if (submissionId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidSubmissionId);

            ValidatePresentationUrl(request.PresentationUrl);

            var submission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultTrackingAsync(s => s.Id == submissionId);

            if (submission is null)
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Submission.CannotUpdateDisqualified);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == submission.TeamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Submission.TeamNotFound);

            if (team.LeaderId != leaderId)
                throw new ForbiddenException(ErrorMessages.Submission.NoUpdatePermission);

            if (team.Status != TeamConstants.Status.Approved)
                throw new BadRequestException(ErrorMessages.Submission.TeamNotApproved);

            var round = await GetRoundOrThrowAsync(submission.RoundId);
            if (!submission.CanEdit)
            {
                ValidateRoundAcceptsSubmissions(round, ErrorMessages.Submission.UpdateDeadlinePassed);
            }

            // Bổ sung kiểm tra RoundTeam khi Update Submission theo yêu cầu (cho Thịnh)
            await EnsureTeamCanSubmitRoundAsync(submission.TeamId, submission.RoundId);

            submission.PresentationUrl = request.PresentationUrl;
            submission.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            return MapToDto(submission);
        }

        public async Task<SubmissionDto> GetSubmissionByIdAsync(Guid submissionId,
            Guid currentAccountId, bool isCoordinator, bool isJudge, bool isMentor)
        {
            if (submissionId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidSubmissionId);

            var submission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission is null)
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            await EnsureCanViewSubmissionAsync(submission, currentAccountId, isCoordinator, isJudge, isMentor);

            return MapToDto(submission);
        }

        public async Task<List<SubmissionDto>> GetSubmissionsByTeamAsync(Guid teamId,
            Guid currentAccountId, bool isCoordinator, bool isMentor)
        {
            if (teamId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidTeamId);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Submission.TeamNotFound);

            var mentorCanViewTeam =
                isMentor
                && await CanMentorViewTeamAsync(team, currentAccountId);

            var canViewSubmissions = isCoordinator
                        || team.LeaderId == currentAccountId
                        || mentorCanViewTeam;

            if (!canViewSubmissions)
                throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);

            var submissions = await _uow.GetRepository<Submission>()
                .GetAllAsync(submission => submission.TeamId == teamId);

            return submissions
                .OrderByDescending(submission => submission.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<List<SubmissionDto>> GetSubmissionsByRoundAsync(int roundId,
            Guid currentAccountId, bool isCoordinator, bool isJudge)
        {
            if (roundId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidRoundId);

            await GetRoundOrThrowAsync(roundId);
            await EnsureCanViewRoundSubmissionsAsync(roundId, currentAccountId, isCoordinator, isJudge);

            // trả về tất cả bài nộp trong Round - bao gồm những bài bị loại
            var submissions = await _uow.GetRepository<Submission>()
                .GetAllAsync(s => s.RoundId == roundId);

            return submissions
                .OrderByDescending(submission => submission.CreatedAt)
                .Select(MapToDto)
                .ToList();
        }

        public async Task DisqualifySubmissionAsync(Guid submissionId,
            DisqualifySubmissionRequest request, Guid coordinatorId)
        {
            if (submissionId == Guid.Empty)
                throw new BadRequestException(
                    ErrorMessages.Common.InvalidSubmissionId);

            var submission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultTrackingAsync(
                    s => s.Id == submissionId);

            if (submission is null)
                throw new NotFoundException(
                    ErrorMessages.Submission.NotFound);

            if (submission.IsDisqualified)
                throw new BadRequestException(
                    ErrorMessages.Submission.AlreadyDisqualified);

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == submission.TeamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(
                    ErrorMessages.Submission.TeamNotFound);

            var reason = request.Reason.Trim();
            var now = DateTime.UtcNow;

            submission.IsDisqualified = true;
            submission.DisqualifyReason = reason;
            submission.DisqualifiedAt = now;
            submission.DisqualifiedBy = coordinatorId;

            // Notification được thêm trước SaveChanges để việc loại bài
            // và tạo thông báo cùng thành công hoặc cùng thất bại.
            await _uow.GetRepository<Notification>().AddAsync(
                new Notification
                {
                    AccountId = team.LeaderId,
                    Title = "Bài nộp đã bị loại",
                    Message = $"Bài nộp của đội {team.TeamName} đã bị loại. Lý do: {reason}",
                    Type = "SUBMISSION_DISQUALIFIED",
                    IsRead = false,
                    CreatedAt = now
                });

            await _uow.SaveChangesAsync();
        }


        // =============== Private helpers ===============
        /// <summary>
        /// Bài nộp của hệ thống cần có link bài thuyết trình/slide.
        /// Link mã nguồn được quản lý ở Team.GithubRepoLink, nên Submission chỉ cần lưu link thuyết trình.
        /// </summary>
        private static void ValidatePresentationUrl(string? presentationUrl)
        {
            if (string.IsNullOrWhiteSpace(presentationUrl))
                throw new BadRequestException(ErrorMessages.Submission.PresentationUrlRequired);
        }

        /// <summary>
        /// Kiểm tra Round có đang ở trạng thái nhận bài không, gồm Active, chưa quá hạn, chưa bắt đầu quá sớm.
        /// </summary>
        private static void ValidateRoundAcceptsSubmissions(Round round, string deadlinePassedErrorMessage)
        {
            // Chỉ cho phép nộp/cập nhật bài khi Round đang mở nhận bài.
            // Active là trạng thái thi chính thức; các trạng thái Upcoming, Scoring, Closed đều không được nhận bài.
            if (!string.Equals(round.Status, RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.Submission.RoundNotActive);

            if (DateTime.UtcNow < round.StartTime)
                throw new BadRequestException(ErrorMessages.Submission.RoundNotStarted);

            if (DateTime.UtcNow > round.EndTime)
                throw new BadRequestException(deadlinePassedErrorMessage);
        }

        /// <summary>
        /// Kiểm tra quyền xem danh sách bài nộp của Round
        /// </summary>
        private async Task EnsureCanViewRoundSubmissionsAsync(int roundId,
            Guid currentAccountId, bool isCoordinator, bool isJudge)
        {
            if (isCoordinator)
                return;

            if (!isJudge)
                throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);

            if (!await CanJudgeViewRoundSubmissionsAsync(roundId, currentAccountId))
                throw new ForbiddenException(ErrorMessages.Submission.JudgeNotAssignedToRound);
        }

        private async Task EnsureCanViewSubmissionAsync(Submission submission, Guid currentAccountId,
            bool isCoordinator, bool isJudge, bool isMentor)
        {
            if (isCoordinator)
                return;

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == submission.TeamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Submission.TeamNotFound);

            if (team.LeaderId == currentAccountId)
                return;

            if (isMentor
                && await CanMentorViewTeamAsync(team, currentAccountId))
            {
                return;
            }

            if (isJudge)
            {
                if (await CanJudgeViewRoundSubmissionsAsync(submission.RoundId, currentAccountId))
                    return;
            }

            throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);
        }

        /// <summary>
        /// Kiểm tra Mentor còn hoạt động trong Event và đang phụ trách đúng Team.
        /// </summary>
        private async Task<bool> CanMentorViewTeamAsync(Team team, Guid mentorId)
        {
            if (team.MentorId != mentorId)
                return false;

            var activeMentorRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(eventAccount =>
                    eventAccount.AccountId == mentorId
                    && eventAccount.EventRole == RoleConstants.Mentor
                    && eventAccount.Status == EventAccountConstants.Status.Approved
                    && !eventAccount.Event.IsDeleted
                    && (eventAccount.Event.Status == EventConstants.Status.Registration
                        || eventAccount.Event.Status == EventConstants.Status.Active)
                    && eventAccount.Event.Tracks.Any(track =>
                        track.Id == team.TrackId && !track.IsDeleted));

            return activeMentorRole is not null;
        }

        /// <summary>
        /// Kiểm tra Judge còn hoạt động trong Event và được phân công vào đúng Round.
        /// </summary>
        private async Task<bool> CanJudgeViewRoundSubmissionsAsync(int roundId, Guid judgeId)
        {
            var judgeAssignment = await _uow.GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(assignment =>
                    assignment.RoundId == roundId
                    && assignment.JudgeId == judgeId
                    && !assignment.Round.Track.IsDeleted
                    && !assignment.Round.Track.Event.IsDeleted
                    && (assignment.Round.Track.Event.Status == EventConstants.Status.Registration
                        || assignment.Round.Track.Event.Status == EventConstants.Status.Active)
                    && assignment.Round.Track.Event.EventAccounts.Any(eventAccount =>
                        eventAccount.AccountId == judgeId
                        && eventAccount.EventRole == RoleConstants.Judge
                        && eventAccount.Status == EventAccountConstants.Status.Approved));

            return judgeAssignment is not null;
        }

        /// <summary>
        /// Lấy Round theo Id và dùng chung cách báo lỗi khi Round không tồn tại.
        /// </summary>
        private async Task<Round> GetRoundOrThrowAsync(int roundId)
        {
            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            return round;
        }

        private async Task EnsureTeamCanSubmitRoundAsync(Guid teamId, int roundId)
        {
            var roundTeam = await _uow.GetRepository<RoundTeam>()
                .GetFirstOrDefaultAsync(rt => rt.RoundId == roundId && rt.TeamId == teamId);

            if (roundTeam is null)
                throw new ForbiddenException(ErrorMessages.Submission.TeamNotQualifiedForRound);
        }

        // =============== Mapping helpers ===============
        private static SubmissionDto MapToDto(Submission submission)
        {
            return new SubmissionDto
            {
                Id = submission.Id,
                TeamId = submission.TeamId,
                RoundId = submission.RoundId,
                PresentationUrl = submission.PresentationUrl,
                IsDisqualified = submission.IsDisqualified,
                DisqualifyReason = submission.DisqualifyReason,
                DisqualifiedAt = submission.DisqualifiedAt,
                DisqualifiedBy = submission.DisqualifiedBy,
                CreatedAt = submission.CreatedAt,
                UpdatedAt = submission.UpdatedAt,
                CanEdit = submission.CanEdit
            };
        }
        
        public async Task<BatchImportResponse<ImportSubmissionSuccessDto, ImportSubmissionSuccessDto>> ImportSubmissionsAsync(int roundId, ImportSubmissionsRequest request)
        {
            var response = new BatchImportResponse<ImportSubmissionSuccessDto, ImportSubmissionSuccessDto>();
            var now = DateTime.UtcNow;

            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);
            
            if (round == null)
            {
                // Nếu không tìm thấy round, tất cả items đều lỗi nhưng ta không thể báo lỗi từng dòng dễ dàng nếu chưa lặp.
                // Ta sẽ lặp và gán lỗi chung.
                foreach (var s in request.Submissions)
                {
                    response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = s.RowNumber, Reason = "RoundId không tồn tại." });
                }
                return response;
            }

            var teamRepo = _uow.GetRepository<Team>();
            var roundTeamRepo = _uow.GetRepository<RoundTeam>();
            var subRepo = _uow.GetRepository<Submission>();
            var topicRepo = _uow.GetRepository<Topic>();

            // Pre-load data to optimize N+1 Query
            var teamIds = request.Submissions.Select(s => s.TeamId).Distinct().ToList();
            var teams = await teamRepo.GetAllAsync(t => teamIds.Contains(t.Id) && !t.IsDeleted);
            var teamDict = teams.ToDictionary(t => t.Id);

            var topicIds = request.Submissions.Where(s => s.TopicId.HasValue).Select(s => s.TopicId!.Value).Distinct().ToList();
            var topics = await topicRepo.GetAllAsync(t => topicIds.Contains(t.Id));
            var topicDict = topics.ToDictionary(t => t.Id);

            var roundTeams = await roundTeamRepo.GetAllAsync(rt => rt.RoundId == roundId && teamIds.Contains(rt.TeamId));
            var roundTeamDict = roundTeams.ToDictionary(rt => rt.TeamId);

            var submissions = await subRepo.GetAllAsync(s => s.RoundId == roundId && teamIds.Contains(s.TeamId));
            var submissionDict = submissions.ToDictionary(s => s.TeamId);

            foreach (var sReq in request.Submissions)
            {
                var rowNumber = sReq.RowNumber;
                try
                {
                    // 1. Validate Team
                    if (!teamDict.TryGetValue(sReq.TeamId, out var team))
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "TeamId không tồn tại hoặc đã bị xóa." });
                        continue;
                    }

                    // 2. Validate TopicId nếu có
                    if (sReq.TopicId.HasValue)
                    {
                        if (!topicDict.TryGetValue(sReq.TopicId.Value, out var topic))
                        {
                            response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "TopicId không tồn tại." });
                            continue;
                        }
                        
                        if (topic.RoundId != roundId)
                        {
                            response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "Topic không thuộc Round hiện tại." });
                            continue;
                        }

                        team.TopicId = topic.Id;
                        team.UpdatedAt = now;
                        teamRepo.Update(team);
                    }

                    // 3. Xử lý RoundTeam
                    if (!roundTeamDict.TryGetValue(team.Id, out var roundTeam))
                    {
                        if (request.AutoCreateRoundTeam)
                        {
                            roundTeam = new RoundTeam
                            {
                                RoundId = roundId,
                                TeamId = team.Id
                            };
                            await roundTeamRepo.AddAsync(roundTeam);
                            // Lưu lại vào Dictionary đề phòng import duplicate team trong cùng mảng
                            roundTeamDict[team.Id] = roundTeam;
                        }
                        else
                        {
                            response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "Team chưa được thêm vào Round này (AutoCreateRoundTeam = false)." });
                            continue;
                        }
                    }

                    // 4. Upsert Submission
                    if (submissionDict.TryGetValue(team.Id, out var submission))
                    {
                        submission.PresentationUrl = sReq.PresentationUrl;
                        submission.UpdatedAt = now;
                        subRepo.Update(submission);
                    }
                    else
                    {
                        submission = new Submission
                        {
                            Id = Guid.NewGuid(),
                            TeamId = team.Id,
                            RoundId = roundId,
                            PresentationUrl = sReq.PresentationUrl,
                            IsDisqualified = false,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        await subRepo.AddAsync(submission);
                    }

                    await _uow.SaveChangesAsync();

                    response.Data.Created.Add(new ImportSubmissionSuccessDto
                    {
                        RowNumber = rowNumber,
                        SubmissionId = submission.Id,
                        TeamId = submission.TeamId,
                        RoundId = submission.RoundId
                    });
                }
                catch (Exception ex)
                {
                    response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = $"Lỗi hệ thống: {ex.Message}" });
                }
            }

            return response;
        }
    }
}
