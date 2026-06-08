using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý logic chấm điểm — tạo ScoreRecord mới và lấy danh sách điểm theo Submission
    /// </summary>
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Judge chấm điểm cho một Submission — validate dữ liệu, kiểm tra quyền, tạo ScoreRecord mới trong DB
        /// </summary>
        public async Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId,
            Guid judgeId,
            SubmitScoreRequest request)
        {
            // Bước 1: Kiểm tra Submission có tồn tại không
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException("Submission", submissionId);

            // Bước 2: Kiểm tra Criterion có tồn tại không
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == request.CriterionId);

            if (criterion == null)
                throw new NotFoundException("Criterion", request.CriterionId);

            if (submission.IsDisqualified)
                throw new BadRequestException("Submission này đã bị loại, không thể chấm điểm.");

            if (criterion.RoundId != submission.RoundId)
                throw new BadRequestException("Criterion không thuộc Round của Submission này.");

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

            if (round is null)
                throw new NotFoundException("Round", submission.RoundId);

            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", round.TrackId);

            var activeJudgeInEvent = await _unitOfWork
                .GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == judgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == "Approved"
                                           && !ea.Event.IsDeleted
                                           && ea.Event.Status == "Active");

            if (activeJudgeInEvent is null)
                throw new ForbiddenException("Tài khoản Judge này không còn hoạt động trong Event của vòng thi.");

            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == judgeId
                                           && ja.RoundId == submission.RoundId);

            if (judgeAssign is null)
                throw new ForbiddenException("Bạn không được phân công chấm vòng thi này.");

            var existingScore = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.SubmissionId == submissionId
                                           && sr.JudgeId == judgeId
                                           && sr.CriterionId == request.CriterionId);

            if (existingScore is not null)
                throw new ConflictException("Bạn đã chấm tiêu chí này cho bài nộp này rồi.");

            // Bước 3: Kiểm tra điểm có hợp lệ không
            if (request.Score < 0 || request.Score > criterion.MaxScore)
                throw new BadRequestException(
                    $"Điểm phải nằm trong khoảng 0 đến {criterion.MaxScore}.");

            // Bước 4: Tạo ScoreRecord mới và lưu vào DB
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

            // Bước 5: Map entity sang Response DTO và trả về
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
        /// Lấy danh sách tất cả điểm đã chấm của một Submission — kèm tên Judge và tên Criterion
        /// </summary>
        public async Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(Guid submissionId)
        {
            // Bước 1: Kiểm tra Submission có tồn tại không
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException("Submission", submissionId);

            // Bước 2: Lấy tất cả ScoreRecord của submission này
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => sr.SubmissionId == submissionId);

            if (!scoreRecords.Any())
                return new List<ScoreRecordResponse>();

            // Bước 3: Lấy thông tin Judge và Criterion
            var judgeIds = scoreRecords.Select(sr => sr.JudgeId).Distinct().ToList();
            var criterionIds = scoreRecords.Select(sr => sr.CriterionId).Distinct().ToList();

            var judges = await _unitOfWork
                .GetRepository<Account>()
                .GetAllAsync(a => judgeIds.Contains(a.Id));

            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => criterionIds.Contains(c.Id));

            // Bước 4: Convert sang Dictionary để lookup nhanh O(1)
            var judgeDict = judges.ToDictionary(a => a.Id, a => a.Username);
            var criterionDict = criteria.ToDictionary(c => c.Id, c => c.Name);

            // Bước 5: Map sang Response DTO
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
    }
}
