using SealHackathon.Application.DTOs.Submission;
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
            ValidateRoundAcceptsSubmissions(round, ErrorMessages.Submission.UpdateDeadlinePassed);

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
                throw new BadRequestException(ErrorMessages.Common.InvalidSubmissionId);

            var submission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultTrackingAsync(s => s.Id == submissionId);

            if (submission is null)
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Submission.AlreadyDisqualified);

            submission.IsDisqualified = true;
            submission.DisqualifyReason = request.Reason;
            submission.DisqualifiedAt = DateTime.UtcNow;
            submission.DisqualifiedBy = coordinatorId;

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
                AiEvaluation = submission.AiEvaluation,
                IsDisqualified = submission.IsDisqualified,
                DisqualifyReason = submission.DisqualifyReason,
                DisqualifiedAt = submission.DisqualifiedAt,
                DisqualifiedBy = submission.DisqualifiedBy,
                CreatedAt = submission.CreatedAt,
                UpdatedAt = submission.UpdatedAt
            };
        }


    }
}
