using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Handles scoring logic — create ScoreRecord and retrieve scores by Submission.
    /// </summary>
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Judge submits a score for a Submission — validates input, checks permissions, creates a new ScoreRecord in DB.
        /// </summary>
        public async Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId, Guid judgeId, SubmitScoreRequest request)
        {
            // Step 1: Verify Submission exists
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Step 2: Verify Criterion exists
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == request.CriterionId);

            if (criterion == null)
                throw new NotFoundException(ErrorMessages.Score.CriterionNotFound);

            // Step 3: Submission must not be disqualified
            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Score.SubmissionDisqualified);

            // Step 4: Criterion must belong to the same Round as the Submission
            if (criterion.RoundId != submission.RoundId)
                throw new BadRequestException(ErrorMessages.Score.CriterionNotInSubmissionRound);

            // Step 5: Verify Round exists
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Score.RoundNotFound);

            // Step 6: Round must be in 'Scoring' status (Active = submission period, Scoring = judging period)
            if (round.Status != RoundConstants.Status.Scoring)
                throw new BadRequestException(ErrorMessages.Score.RoundNotInScoring);

            // Step 7: Verify Track exists and is not deleted
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Score.TrackNotFound);

            // Step 8: Judge must be an active EventAccount (Approved) in an Active Event
            var activeJudgeInEvent = await _unitOfWork
                .GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == judgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == EventAccountConstants.Status.Approved
                                           && !ea.Event.IsDeleted
                                           && ea.Event.Status == EventConstants.Status.Active);

            if (activeJudgeInEvent is null)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNotActiveInEvent);

            // Step 9: Judge must be assigned to this Round
            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == judgeId
                                           && ja.RoundId == submission.RoundId);

            if (judgeAssign is null)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNotAssignedToRound);

            // Step 10: Prevent duplicate scoring (same judge / criterion / submission)
            var existingScore = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.SubmissionId == submissionId
                                           && sr.JudgeId == judgeId
                                           && sr.CriterionId == request.CriterionId);

            if (existingScore is not null)
                throw new ConflictException(ErrorMessages.Score.AlreadyScored);

            // Step 11: Validate score value
            if (request.Score < 0 || request.Score > criterion.MaxScore)
                throw new BadRequestException(ErrorMessages.Score.InvalidScoreRange);

            // Step 12: Create and persist ScoreRecord
            var scoreRecord = new ScoreRecord
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                JudgeId = judgeId,
                CriterionId = request.CriterionId,
                Score = request.Score,
                Comment = request.Comment,
                IsCalibration = request.IsCalibration,
                ScoredAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<ScoreRecord>().AddAsync(scoreRecord);
            await _unitOfWork.SaveChangesAsync();

            // Step 13: Return response DTO
            return new ScoreRecordResponse
            {
                Id = scoreRecord.Id,
                SubmissionId = scoreRecord.SubmissionId,
                JudgeId = scoreRecord.JudgeId,
                JudgeName = string.Empty,
                CriterionId = scoreRecord.CriterionId,
                CriterionName = criterion.Name,
                Score = scoreRecord.Score,
                Comment = scoreRecord.Comment,
                IsCalibration = scoreRecord.IsCalibration,
                ScoredAt = scoreRecord.ScoredAt
            };
        }

        /// <summary>
        /// Retrieves all scores for a Submission — includes Judge name and Criterion name.
        /// </summary>
        public async Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(Guid submissionId)
        {
            // Step 1: Verify Submission exists
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Step 2: Fetch all ScoreRecords for this Submission
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => sr.SubmissionId == submissionId);

            if (!scoreRecords.Any())
                return new List<ScoreRecordResponse>();

            // Step 3: Fetch Judge accounts and Criteria for name lookup
            var judgeIds = scoreRecords.Select(sr => sr.JudgeId).Distinct().ToList();
            var criterionIds = scoreRecords.Select(sr => sr.CriterionId).Distinct().ToList();

            var judges = await _unitOfWork
                .GetRepository<Account>()
                .GetAllAsync(a => judgeIds.Contains(a.Id));

            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => criterionIds.Contains(c.Id));

            // Step 4: Build O(1) lookup dictionaries
            var judgeDict = judges.ToDictionary(a => a.Id, a => a.Username);
            var criterionDict = criteria.ToDictionary(c => c.Id, c => c.Name);

            // Step 5: Map to response DTOs
            var result = scoreRecords.Select(sr => new ScoreRecordResponse
            {
                Id = sr.Id,
                SubmissionId = sr.SubmissionId,
                JudgeId = sr.JudgeId,
                JudgeName = judgeDict.GetValueOrDefault(sr.JudgeId, string.Empty),
                CriterionId = sr.CriterionId,
                CriterionName = criterionDict.GetValueOrDefault(sr.CriterionId, string.Empty),
                Score = sr.Score,
                Comment = sr.Comment,
                IsCalibration = sr.IsCalibration,
                ScoredAt = sr.ScoredAt
            }).ToList();

            return result;
        }

        /// <summary>
        /// Judge updates a previously submitted score — re-validates permissions and round/submission state.
        /// </summary>
        public async Task<ScoreRecordResponse> UpdateScoreAsync(
            Guid scoreRecordId,
            Guid judgeId,
            UpdateScoreRequest request)
        {
            // Step 1: Verify ScoreRecord exists
            var scoreRecord = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.Id == scoreRecordId);

            if (scoreRecord == null)
                throw new NotFoundException(ErrorMessages.Score.NotFound);

            // Step 2: Judge can only edit their own scores
            if (scoreRecord.JudgeId != judgeId)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNoUpdatePermission);

            // Step 3: Verify Submission exists
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == scoreRecord.SubmissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Step 4: Submission must not be disqualified
            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Score.SubmissionDisqualifiedCannotUpdate);

            // Step 5: Verify Round exists
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

            if (round == null)
                throw new NotFoundException(ErrorMessages.Score.RoundNotFound);

            // Step 6: Round must still be in 'Scoring' status
            if (round.Status != RoundConstants.Status.Scoring)
                throw new BadRequestException(ErrorMessages.Score.RoundNotInScoring);

            // Step 7: Verify Track exists and is not deleted
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Score.TrackNotFound);

            // Step 8: Re-check EventAccount — Judge may have been deactivated since last scoring
            var activeJudgeInEvent = await _unitOfWork
                .GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == judgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == EventAccountConstants.Status.Approved
                                           && !ea.Event.IsDeleted
                                           && ea.Event.Status == EventConstants.Status.Active);

            if (activeJudgeInEvent is null)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNotActiveInEvent);

            // Step 9: Re-check JudgeAssign — Judge may have been unassigned from this Round
            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == judgeId
                                           && ja.RoundId == submission.RoundId);

            if (judgeAssign is null)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNotAssignedToRound);

            // Step 10: Verify Criterion exists and validate new score value
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == scoreRecord.CriterionId);

            if (criterion == null)
                throw new NotFoundException(ErrorMessages.Score.CriterionNotFound);

            if (request.UpdatedScore < 0 || request.UpdatedScore > criterion.MaxScore)
                throw new BadRequestException(ErrorMessages.Score.InvalidScoreRange);

            // Step 11: Apply update
            scoreRecord.Score = request.UpdatedScore;
            scoreRecord.Comment = request.UpdatedComment;
            scoreRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<ScoreRecord>().Update(scoreRecord);
            await _unitOfWork.SaveChangesAsync();

            // Step 12: Return updated response DTO
            return new ScoreRecordResponse
            {
                Id = scoreRecord.Id,
                SubmissionId = scoreRecord.SubmissionId,
                JudgeId = scoreRecord.JudgeId,
                JudgeName = string.Empty,
                CriterionId = scoreRecord.CriterionId,
                CriterionName = criterion.Name,
                Score = scoreRecord.Score,
                Comment = scoreRecord.Comment,
                IsCalibration = scoreRecord.IsCalibration,
                ScoredAt = scoreRecord.ScoredAt
            };
        }
    }
}
