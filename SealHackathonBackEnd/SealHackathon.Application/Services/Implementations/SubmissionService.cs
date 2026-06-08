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

            if (string.IsNullOrWhiteSpace(request.DemoUrl)
                && string.IsNullOrWhiteSpace(request.ReportUrl))
                throw new BadRequestException(ErrorMessages.Submission.NeedAtLeastOneLink);

            var round = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            if (DateTime.UtcNow > round.EndTime)
                throw new BadRequestException(ErrorMessages.Submission.DeadlinePassed);

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

            var existingSubmission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.TeamId == team.Id && s.RoundId == round.Id);

            if (existingSubmission is not null)
                throw new ConflictException(ErrorMessages.Submission.AlreadySubmitted);

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                RoundId = round.Id,
                DemoUrl = request.DemoUrl,
                ReportUrl = request.ReportUrl,
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
            if (submissionId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidSubmissionId);

            if (string.IsNullOrWhiteSpace(request.DemoUrl)
                && string.IsNullOrWhiteSpace(request.ReportUrl))
                throw new BadRequestException(ErrorMessages.Submission.NeedAtLeastOneLink);

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

            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            if (DateTime.UtcNow > round.EndTime)
                throw new BadRequestException(ErrorMessages.Submission.UpdateDeadlinePassed);

            submission.DemoUrl = request.DemoUrl;
            submission.ReportUrl = request.ReportUrl;

            await _uow.SaveChangesAsync();

            return MapToDto(submission);
        }

        public async Task<SubmissionDto> GetSubmissionByIdAsync(Guid submissionId, 
            Guid currentAccountId, bool isCoordinator, bool isJudge)
        {
            var submission = await _uow.GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission is null)
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            await EnsureCanViewSubmissionAsync(submission, currentAccountId, isCoordinator, isJudge);

            return MapToDto(submission);
        }

        public async Task<List<SubmissionDto>> GetSubmissionsByTeamAsync(Guid teamId, 
            Guid currentAccountId, bool isCoordinator)
        {
            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Submission.TeamNotFound);

            if (!isCoordinator && team.LeaderId != currentAccountId)
                throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);

            var submissions = await _uow.GetRepository<Submission>()
                .GetAllAsync(s => s.TeamId == teamId);

            return submissions.Select(MapToDto).ToList();
        }

        public async Task<List<SubmissionDto>> GetSubmissionsByRoundAsync(int roundId,
            Guid currentAccountId, bool isCoordinator, bool isJudge)
        {
            if (roundId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidRoundId);

            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            if (!isCoordinator)
            {
                if (!isJudge)
                    throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);

                var judgeAssign = await _uow.GetRepository<JudgeAssign>()
                    .GetFirstOrDefaultAsync(ja => ja.RoundId == roundId
                                               && ja.JudgeId == currentAccountId);

                if (judgeAssign is null)
                    throw new ForbiddenException(ErrorMessages.Submission.JudgeNotAssignedToRound);
            }

            var submissions = await _uow.GetRepository<Submission>()
                .GetAllAsync(s => s.RoundId == roundId);

            return submissions.Select(MapToDto).ToList();
        }

        public async Task DisqualifySubmissionAsync(Guid submissionId, 
            DisqualifySubmissionRequest request, Guid coordinatorId)
        {
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

        private async Task EnsureCanViewSubmissionAsync(Submission submission, Guid currentAccountId,
            bool isCoordinator, bool isJudge)
        {
            if (isCoordinator)
                return;

            var team = await _uow.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == submission.TeamId && !t.IsDeleted);

            if (team is null)
                throw new NotFoundException(ErrorMessages.Submission.TeamNotFound);

            if (team.LeaderId == currentAccountId)
                return;

            if (isJudge)
            {
                var judgeAssign = await _uow.GetRepository<JudgeAssign>()
                    .GetFirstOrDefaultAsync(ja => ja.RoundId == submission.RoundId
                                               && ja.JudgeId == currentAccountId);

                if (judgeAssign is not null)
                    return;
            }

            throw new ForbiddenException(ErrorMessages.Submission.NoViewPermission);
        }

        private static SubmissionDto MapToDto(Submission submission)
        {
            return new SubmissionDto
            {
                Id = submission.Id,
                TeamId = submission.TeamId,
                RoundId = submission.RoundId,
                DemoUrl = submission.DemoUrl,
                ReportUrl = submission.ReportUrl,
                AiEvaluation = submission.AiEvaluation,
                IsDisqualified = submission.IsDisqualified,
                DisqualifyReason = submission.DisqualifyReason,
                DisqualifiedAt = submission.DisqualifiedAt,
                DisqualifiedBy = submission.DisqualifiedBy,
                CreatedAt = submission.CreatedAt
            };
        }
    }
}
