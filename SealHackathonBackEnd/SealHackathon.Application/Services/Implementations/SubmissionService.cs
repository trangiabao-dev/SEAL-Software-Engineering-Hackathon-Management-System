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
        
        /// <summary>
        /// Import danh sách bài thi (Submissions) bằng Excel/CSV.
        /// Kiểm tra chặt chẽ điều kiện vòng thi và trạng thái đội thi trước khi import, đảm bảo an toàn ChangeTracker.
        /// </summary>
        public async Task<BatchImportResponse<ImportSubmissionSuccessDto, ImportSubmissionSuccessDto>> ImportSubmissionsAsync(int roundId, ImportSubmissionsRequest request)
        {
            var response = new BatchImportResponse<ImportSubmissionSuccessDto, ImportSubmissionSuccessDto>();
            var now = DateTime.UtcNow;

            var round = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(r => r.Id == roundId);
            if (round == null)
            {
                response.Success = false;
                response.Message = "Vòng thi không tồn tại.";
                return response;
            }

            // Kiểm tra trạng thái Vòng thi: Nếu đã đóng hoặc đang chấm điểm thì từ chối import
            if (round.Status == RoundConstants.Status.Closed || round.Status == RoundConstants.Status.Scoring)
            {
                response.Success = false;
                response.Message = $"Vòng thi đang ở trạng thái '{round.Status}', không cho phép import hay thay đổi bài làm.";
                return response;
            }

            var teamRepo = _uow.GetRepository<Team>();
            var roundTeamRepo = _uow.GetRepository<RoundTeam>();
            var subRepo = _uow.GetRepository<Submission>();
            var topicRepo = _uow.GetRepository<Topic>();

            // Tối ưu N+1 Query: tải trước dữ liệu Teams, Topics, RoundTeams, Submissions
            var teamIds = request.Submissions.Select(s => s.TeamId).Distinct().ToList();
            var teams = await teamRepo.GetAllAsync(t => teamIds.Contains(t.Id) && !t.IsDeleted);
            var teamDict = teams.ToDictionary(t => t.Id);

            var topicIds = request.Submissions.Where(s => s.TopicId.HasValue)
                .Select(s => s.TopicId!.Value).Distinct().ToList();

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
                    // Kiểm tra tính hợp lệ của dòng dữ liệu import
                    var validationError = ValidateSubmissionRow(roundId, sReq, teamDict, topicDict, out var team);
                    if (validationError != null)
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = validationError });
                        continue;
                    }

                    // Cập nhật đề tài (Topic) nếu có
                    if (sReq.TopicId.HasValue)
                    {
                        var topic = topicDict[sReq.TopicId.Value];
                        team!.TopicId = topic.Id;
                        team.UpdatedAt = now;
                    }

                    // Đảm bảo nhóm đã được thêm vào vòng thi (RoundTeam) trước khi nộp bài
                    var roundTeamError = await EnsureRoundTeamExistsAsync(roundId, sReq.TeamId, 
                        request.AutoCreateRoundTeam, roundTeamDict, roundTeamRepo);
                    if (roundTeamError != null)
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = roundTeamError });
                        continue;
                    }

                    // Thêm mới hoặc cập nhật bài thi (Upsert)
                    var submission = UpsertSubmissionEntity(roundId, sReq.TeamId, 
                        sReq.PresentationUrl, submissionDict, subRepo, now);

                    await _uow.SaveChangesAsync();

                    if (submissionDict.ContainsKey(sReq.TeamId))
                    {
                        response.Data.Updated.Add(new ImportSubmissionSuccessDto
                        {
                            RowNumber = rowNumber,
                            SubmissionId = submission.Id,
                            TeamId = submission.TeamId,
                            RoundId = submission.RoundId
                        });
                    }
                    else
                    {
                        submissionDict[sReq.TeamId] = submission;
                        response.Data.Created.Add(new ImportSubmissionSuccessDto
                        {
                            RowNumber = rowNumber,
                            SubmissionId = submission.Id,
                            TeamId = submission.TeamId,
                            RoundId = submission.RoundId
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Dọn sạch bộ nhớ ChangeTracker để tránh entity lỗi làm sập dây chuyền các dòng sau
                    _uow.ClearChangeTracker();
                    response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = $"Lỗi hệ thống: {ex.Message}" });
                }
            }

            return response;
        }

        /// <summary>
        /// Kiểm tra hợp lệ dòng import bài thi: điều kiện đội thi tham gia giải, link thuyết trình và đề tài.
        /// </summary>
        private string? ValidateSubmissionRow(
            int roundId,
            ImportSubmissionDto sReq,
            Dictionary<Guid, Team> teamDict,
            Dictionary<int, Topic> topicDict,
            out Team? team)
        {
            if (!teamDict.TryGetValue(sReq.TeamId, out team))
            {
                return "TeamId không tồn tại hoặc đã bị xóa.";
            }

            if (string.IsNullOrWhiteSpace(sReq.PresentationUrl))
            {
                return "Link thuyết trình (PresentationUrl) không được để trống.";
            }

            // Kiểm tra Đội thi đã bị loại khỏi giải hay chưa
            if (team.Status == TeamConstants.Status.Disqualified)
            {
                return "Đội thi này đã bị Ban tổ chức loại khỏi giải đấu, không được phép nộp bài.";
            }

            if (string.IsNullOrWhiteSpace(team.GithubRepoLink))
            {
                return "Đội thi chưa cập nhật link Github Repository, không đủ điều kiện nộp bài.";
            }

            if (sReq.TopicId.HasValue)
            {
                if (!topicDict.TryGetValue(sReq.TopicId.Value, out var topic))
                {
                    return "TopicId không tồn tại.";
                }

                if (topic.RoundId != roundId)
                {
                    return "Đề tài (Topic) không thuộc Vòng thi hiện tại.";
                }
            }

            return null;
        }

        /// <summary>
        /// Đảm bảo đội thi đã thuộc Vòng thi (RoundTeam) trước khi nộp bài, 
        /// tự động đăng ký nếu AutoCreateRoundTeam = true.
        /// </summary>
        private async Task<string?> EnsureRoundTeamExistsAsync(
            int roundId,
            Guid teamId,
            bool autoCreateRoundTeam,
            Dictionary<Guid, RoundTeam> roundTeamDict,
            IGenericRepository<RoundTeam> roundTeamRepo)
        {
            if (roundTeamDict.ContainsKey(teamId))
            {
                return null;
            }

            if (!autoCreateRoundTeam)
            {
                return "Đội thi chưa được thêm vào Vòng thi này (AutoCreateRoundTeam = false).";
            }

            var newRoundTeam = new RoundTeam
            {
                RoundId = roundId,
                TeamId = teamId
            };
            await roundTeamRepo.AddAsync(newRoundTeam);
            roundTeamDict[teamId] = newRoundTeam;

            return null;
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật bài thi (Upsert): Cập nhật link bài làm nếu đã tồn tại, khởi tạo mới nếu chưa có.
        /// </summary>
        private Submission UpsertSubmissionEntity(
            int roundId,
            Guid teamId,
            string presentationUrl,
            Dictionary<Guid, Submission> submissionDict,
            IGenericRepository<Submission> subRepo,
            DateTime now)
        {
            if (submissionDict.TryGetValue(teamId, out var existingSub))
            {
                existingSub.PresentationUrl = presentationUrl;
                existingSub.UpdatedAt = now;
                subRepo.Update(existingSub);
                return existingSub;
            }

            var newSub = new Submission
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                RoundId = roundId,
                PresentationUrl = presentationUrl,
                IsDisqualified = false,
                CreatedAt = now,
                UpdatedAt = now
            };
            subRepo.AddAsync(newSub);
            submissionDict[teamId] = newSub;
            return newSub;
        }
    }
}
