using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý nghiệp vụ chấm điểm: tạo điểm, cập nhật điểm và lấy danh sách điểm theo bài nộp.
    /// </summary>
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Judge chấm điểm cho một bài nộp: kiểm tra dữ liệu, kiểm tra quyền chấm và tạo ScoreRecord.
        /// </summary>
        public async Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId, Guid judgeId, SubmitScoreRequest request)
        {
            // Kiểm tra bài nộp có tồn tại.
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Kiểm tra tiêu chí chấm điểm có tồn tại.
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == request.CriterionId);

            if (criterion == null)
                throw new NotFoundException(ErrorMessages.Score.CriterionNotFound);

            // Không cho chấm bài nộp đã bị loại.
            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Score.SubmissionDisqualified);

            // Lớp bảo vệ thứ hai: kiểm tra chính Team có bị loại không.
            // Phòng trường hợp submission được tạo sau khi team bị loại mà chưa set IsDisqualified.
            await EnsureTeamNotDisqualifiedAsync(
                submission.TeamId, ErrorMessages.Score.TeamDisqualified);

            // Tiêu chí phải thuộc cùng Round với bài nộp.
            if (criterion.RoundId != submission.RoundId)
                throw new BadRequestException(ErrorMessages.Score.CriterionNotInSubmissionRound);

            // Round phải tồn tại và đang ở trạng thái Scoring.
            var round = await GetScoringRoundAsync(submission.RoundId);

            // Judge phải còn quyền Judge trong Event và được phân công vào Round này.
            await EnsureJudgeCanScoreRoundAsync(judgeId, round);

            // Không cho cùng Judge chấm trùng cùng tiêu chí của cùng bài nộp.
            var existingScore = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.SubmissionId == submissionId
                                           && sr.JudgeId == judgeId
                                           && sr.CriterionId == request.CriterionId);

            if (existingScore is not null)
                throw new ConflictException(ErrorMessages.Score.AlreadyScored);

            // Kiểm tra điểm nhập vào không vượt quá điểm tối đa.
            if (request.Score < 0 || request.Score > criterion.MaxScore)
                throw new BadRequestException(ErrorMessages.Score.InvalidScoreRange);

            // Tạo ScoreRecord và lưu xuống database.
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

            // Trả kết quả cho FE.
            return MapToScoreRecordResponse(scoreRecord, criterion.Name);
        }

        /// <summary>
        /// Lấy toàn bộ điểm của một bài nộp, kèm tên Judge và tên tiêu chí.
        /// </summary>
        public async Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(
            Guid submissionId, Guid currentAccountId, bool isCoordinator)
        {
            // Kiểm tra bài nộp có tồn tại.
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Nếu người xem không phải Coordinator thì phải là Judge được phân công vào Round của bài nộp.
            if (!isCoordinator)
            {
                var round = await _unitOfWork
                    .GetRepository<Round>()
                    .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

                if (round is null)
                    throw new NotFoundException(ErrorMessages.Score.RoundNotFound);

                await EnsureJudgeCanScoreRoundAsync(currentAccountId, round);
            }

            // Lấy toàn bộ ScoreRecord của bài nộp.
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => sr.SubmissionId == submissionId);

            if (!scoreRecords.Any())
                return new List<ScoreRecordResponse>();

            // Lấy danh sách Judge và tiêu chí để hiển thị tên.
            var judgeIds = scoreRecords.Select(sr => sr.JudgeId).Distinct().ToList();
            var criterionIds = scoreRecords.Select(sr => sr.CriterionId).Distinct().ToList();

            var judges = await _unitOfWork
                .GetRepository<Account>()
                .GetAllAsync(a => judgeIds.Contains(a.Id));

            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => criterionIds.Contains(c.Id));

            // Tạo dictionary để tra cứu tên nhanh hơn.
            var judgeDict = judges.ToDictionary(a => a.Id, a => a.Username);
            var criterionDict = criteria.ToDictionary(c => c.Id, c => c.Name);

            // Chuyển dữ liệu sang response DTO.
            var result = scoreRecords
                .Select(sr => MapToScoreRecordResponse(
                    sr, criterionDict.GetValueOrDefault(sr.CriterionId, string.Empty),
                    judgeDict.GetValueOrDefault(sr.JudgeId, string.Empty)))
                .ToList();

            return result;
        }

        /// <summary>
        /// Judge cập nhật điểm đã chấm: kiểm tra quyền, trạng thái Round và trạng thái bài nộp.
        /// </summary>
        public async Task<ScoreRecordResponse> UpdateScoreAsync(
            Guid scoreRecordId,
            Guid judgeId,
            UpdateScoreRequest request)
        {
            // Kiểm tra ScoreRecord có tồn tại.
            var scoreRecord = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.Id == scoreRecordId);

            if (scoreRecord == null)
                throw new NotFoundException(ErrorMessages.Score.NotFound);

            // Judge chỉ được sửa điểm do chính mình chấm.
            if (scoreRecord.JudgeId != judgeId)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNoUpdatePermission);

            // Kiểm tra bài nộp có tồn tại.
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == scoreRecord.SubmissionId);

            if (submission == null)
                throw new NotFoundException(ErrorMessages.Score.SubmissionNotFound);

            // Không cho sửa điểm của bài nộp đã bị loại.
            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Score.SubmissionDisqualifiedCannotUpdate);

            // Lớp bảo vệ thứ hai: kiểm tra chính Team có bị loại không.
            await EnsureTeamNotDisqualifiedAsync(
                submission.TeamId, ErrorMessages.Score.TeamDisqualifiedCannotUpdate);

            // Round phải tồn tại và đang ở trạng thái Scoring.
            var round = await GetScoringRoundAsync(submission.RoundId);

            // Judge phải còn quyền Judge trong Event và được phân công vào Round này.
            await EnsureJudgeCanScoreRoundAsync(judgeId, round);

            // Kiểm tra tiêu chí có tồn tại và điểm mới có hợp lệ không.
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == scoreRecord.CriterionId);

            if (criterion == null)
                throw new NotFoundException(ErrorMessages.Score.CriterionNotFound);

            if (request.UpdatedScore < 0 || request.UpdatedScore > criterion.MaxScore)
                throw new BadRequestException(ErrorMessages.Score.InvalidScoreRange);

            // Cập nhật điểm.
            scoreRecord.Score = request.UpdatedScore;
            scoreRecord.Comment = request.UpdatedComment;
            scoreRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<ScoreRecord>().Update(scoreRecord);
            await _unitOfWork.SaveChangesAsync();

            // Trả kết quả đã cập nhật cho FE.
            return MapToScoreRecordResponse(scoreRecord, criterion.Name);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Lớp bảo vệ thứ hai (defense in depth): kiểm tra Team có bị Disqualified không.
        /// Phòng trường hợp Submission lọt qua mà chưa được đánh cờ IsDisqualified.
        /// </summary>
        private async Task EnsureTeamNotDisqualifiedAsync(Guid teamId, string errorMessage)
        {
            var team = await _unitOfWork
                .GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId && !t.IsDeleted);

            if (team is not null
                && string.Equals(team.Status, TeamConstants.Status.Disqualified, StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(errorMessage);
            }
        }

        // Kiểm tra Round có tồn tại và đang ở trạng thái Scoring.
        private async Task<Round> GetScoringRoundAsync(int roundId)
        {
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Score.RoundNotFound);

            if (!string.Equals(round.Status, RoundConstants.Status.Scoring, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.Score.RoundNotInScoring);

            return round;
        }

        // Kiểm tra Judge còn quyền trong Event và được phân công vào Round.
        private async Task EnsureJudgeCanScoreRoundAsync(Guid judgeId, Round round)
        {
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Score.TrackNotFound);

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

            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == judgeId
                                           && ja.RoundId == round.Id);

            if (judgeAssign is null)
                throw new ForbiddenException(ErrorMessages.Score.JudgeNotAssignedToRound);
        }

        private static ScoreRecordResponse MapToScoreRecordResponse(
            ScoreRecord scoreRecord, string criterionName, string judgeName = "")
        {
            return new ScoreRecordResponse
            {
                Id = scoreRecord.Id,
                SubmissionId = scoreRecord.SubmissionId,
                JudgeId = scoreRecord.JudgeId,
                JudgeName = judgeName,
                CriterionId = scoreRecord.CriterionId,
                CriterionName = criterionName,
                Score = scoreRecord.Score,
                Comment = scoreRecord.Comment,
                IsCalibration = scoreRecord.IsCalibration,
                ScoredAt = scoreRecord.ScoredAt
            };
        }
    }
}
