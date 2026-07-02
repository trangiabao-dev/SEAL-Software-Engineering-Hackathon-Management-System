using SealHackathon.Application.Common.Calculations;
using SealHackathon.Application.Common.Requests;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.DTOs.Batch;
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
        private readonly IAuditLogService _auditLogService;

        /// <summary>
        /// Khởi tạo service xử lý chấm điểm và ghi lịch sử thay đổi điểm.
        /// </summary>
        public ScoreService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
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

            // Ranking đã tính thì điểm phải bị khóa để tránh lệch với TotalScore đã công bố.
            await EnsureRankingNotCalculatedAsync(round.Id);

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
                ScoredAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<ScoreRecord>().AddAsync(scoreRecord);

            // Đưa ScoreRecord và AuditLog vào cùng Unit of Work để hai bản ghi được lưu nguyên tử.
            await _auditLogService.AddAsync(
                judgeId,
                AuditActionConstants.ScoreAudit.Create,
                nameof(ScoreRecord),
                scoreRecord.Id.ToString(),
                newValues: CreateScoreAuditValues(scoreRecord));

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

            // Judge chỉ xem điểm của mình; Coordinator vẫn xem toàn bộ điểm của Submission.
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(scoreRecord =>
                    scoreRecord.SubmissionId == submissionId
                    && (isCoordinator || scoreRecord.JudgeId == currentAccountId));

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
        /// Lấy lịch sử chấm bài có phân trang của Judge hiện tại.
        /// </summary>
        public async Task<PaginatedResponse<JudgeScoreHistoryResponse>> GetMyScoreHistoryAsync(
            Guid judgeId,
            int pageNumber,
            int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);

            // Include dữ liệu liên quan một lần để tránh query riêng từng Submission.
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllWithIncludeAsync(
                    scoreRecord =>
                        scoreRecord.JudgeId == judgeId,
                      
                    scoreRecord => scoreRecord.Submission.Team,
                    scoreRecord => scoreRecord.Submission.Round.Criteria);

            var scoreGroups = scoreRecords
                .GroupBy(scoreRecord => scoreRecord.SubmissionId)
                .OrderByDescending(group =>
                    group.Max(scoreRecord => scoreRecord.UpdatedAt ?? scoreRecord.ScoredAt))
                .ToList();

            var pagedScoreGroups = scoreGroups
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (pagedScoreGroups.Count == 0)
            {
                return new PaginatedResponse<JudgeScoreHistoryResponse>(
                    new List<JudgeScoreHistoryResponse>(),
                    scoreGroups.Count,
                    pageNumber,
                    pageSize);
            }

            var roundIds = pagedScoreGroups
                .Select(group => group.First().Submission.RoundId)
                .Distinct()
                .ToList();

            // Ranking chỉ cần query theo các Round xuất hiện trong trang hiện tại.
            var rankedRoundIds = (await _unitOfWork
                    .GetRepository<Ranking>()
                    .GetAllAsync(ranking => roundIds.Contains(ranking.RoundId)))
                .Select(ranking => ranking.RoundId)
                .ToHashSet();

            var items = pagedScoreGroups
                .Select(group => MapToJudgeScoreHistoryResponse(group, rankedRoundIds))
                .ToList();

            return new PaginatedResponse<JudgeScoreHistoryResponse>(
                items,
                scoreGroups.Count,
                pageNumber,
                pageSize);
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

            // Ranking đã tính thì không cho sửa ScoreRecord cũ.
            await EnsureRankingNotCalculatedAsync(round.Id);

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

            // Chụp dữ liệu cũ trước khi thay đổi để lịch sử phản ánh đúng trạng thái ban đầu.
            var oldValues = CreateScoreAuditValues(scoreRecord);

            // Cập nhật điểm.
            scoreRecord.Score = request.UpdatedScore;
            scoreRecord.Comment = request.UpdatedComment;
            scoreRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<ScoreRecord>().Update(scoreRecord);

            // ScoreRecord và AuditLog phải thành công hoặc thất bại cùng nhau.
            await _auditLogService.AddAsync(
                judgeId,
                AuditActionConstants.ScoreAudit.Update,
                nameof(ScoreRecord),
                scoreRecord.Id.ToString(),
                oldValues,
                CreateScoreAuditValues(scoreRecord));

            await _unitOfWork.SaveChangesAsync();

            // Trả kết quả đã cập nhật cho FE.
            return MapToScoreRecordResponse(scoreRecord, criterion.Name);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Kiểm tra tham số phân trang trước khi query dữ liệu.
        /// </summary>
        private static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageNumber);

            if (pageSize < 1 || pageSize > PaginationRequest.MaxPageSize)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageSize);
        }

        /// <summary>
        /// Chuyển một nhóm ScoreRecord của cùng Submission thành một dòng lịch sử.
        /// </summary>
        private static JudgeScoreHistoryResponse MapToJudgeScoreHistoryResponse(
            IGrouping<Guid, ScoreRecord> scoreGroup,
            HashSet<int> rankedRoundIds)
        {
            var scoreRecords = scoreGroup.ToList();
            var submission = scoreRecords[0].Submission;
            var round = submission.Round;
            var team = submission.Team;
            var criteria = round.Criteria.ToList();
            var scoreByCriterionId = scoreRecords.ToDictionary(
                scoreRecord => scoreRecord.CriterionId);

            var scoredCriteriaCount = criteria.Count(
                criterion => scoreByCriterionId.ContainsKey(criterion.Id));
            var isCompleted = criteria.Count > 0
                && scoredCriteriaCount == criteria.Count;

            double? myScore = isCompleted
                ? CalculateMyScore(scoreByCriterionId, criteria)
                : null;

            return new JudgeScoreHistoryResponse
            {
                SubmissionId = submission.Id,
                TeamId = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                RoundId = round.Id,
                RoundName = round.Name,
                SubmittedAt = submission.CreatedAt,
                LastScoredAt = scoreRecords.Max(scoreRecord =>
                    scoreRecord.UpdatedAt ?? scoreRecord.ScoredAt),
                MyScore = myScore,
                ScoredCriteriaCount = scoredCriteriaCount,
                TotalCriteriaCount = criteria.Count,
                Status = ResolveScoreHistoryStatus(
                    submission,
                    round,
                    rankedRoundIds.Contains(round.Id),
                    isCompleted)
            };
        }

        /// <summary>
        /// Tính điểm tổng có trọng số của Judge từ các Criterion đã chấm đủ.
        /// </summary>
        private static double CalculateMyScore(
            IReadOnlyDictionary<int, ScoreRecord> scoreByCriterionId,
            IReadOnlyCollection<Criterion> criteria)
        {
            var myScore = criteria.Sum(criterion =>
                ScoreCalculation.CalculateWeightedCriterionScore(
                    scoreByCriterionId[criterion.Id].Score,
                    criterion.MaxScore,
                    criterion.Weight));

            return Math.Round(myScore, 2);
        }

        /// <summary>
        /// Xác định trạng thái hiển thị của một dòng lịch sử chấm bài.
        /// </summary>
        private static string ResolveScoreHistoryStatus(
            Submission submission,
            Round round,
            bool hasRanking,
            bool isCompleted)
        {
            if (submission.IsDisqualified)
                return ScoreHistoryConstants.Status.Disqualified;

            // ScoreService chỉ cho sửa khi Round đang Scoring và chưa có Ranking.
            if (hasRanking
                || !string.Equals(
                    round.Status,
                    RoundConstants.Status.Scoring,
                    StringComparison.OrdinalIgnoreCase))
            {
                return ScoreHistoryConstants.Status.Locked;
            }

            return isCompleted
                ? ScoreHistoryConstants.Status.Completed
                : ScoreHistoryConstants.Status.InProgress;
        }

        /// <summary>
        /// Tạo bản chụp dữ liệu chấm điểm cần lưu vào AuditLog.
        /// Chỉ lấy thuộc tính cần thiết để tránh serialize navigation property.
        /// </summary>
        private static object CreateScoreAuditValues(ScoreRecord scoreRecord)
        {
            return new
            {
                scoreRecord.SubmissionId,
                scoreRecord.JudgeId,
                scoreRecord.CriterionId,
                scoreRecord.Score,
                scoreRecord.Comment,
            };
        }

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

        /// <summary>
        /// Chặn tạo hoặc cập nhật điểm nếu round đã có kết quả ranking.
        /// </summary>
        private async Task EnsureRankingNotCalculatedAsync(int roundId)
        {
            var existingRanking = await _unitOfWork
                .GetRepository<Ranking>()
                .GetFirstOrDefaultAsync(r => r.RoundId == roundId);

            if (existingRanking is not null)
                throw new BadRequestException(ErrorMessages.Score.RankingAlreadyCalculated);
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
                ScoredAt = scoreRecord.ScoredAt
            };
        }
        public async Task<BatchImportResponse<ImportScoreSuccessDto, ImportScoreSuccessDto>> ImportScoresAsync(int roundId, ImportScoresRequest request, Guid coordinatorId)
        {
            var response = new BatchImportResponse<ImportScoreSuccessDto, ImportScoreSuccessDto>();
            var now = DateTime.UtcNow;

            var round = await _unitOfWork.GetRepository<Round>().GetFirstOrDefaultAsync(r => r.Id == roundId);
            if (round == null)
            {
                foreach (var s in request.Scores)
                    response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = s.RowNumber, Reason = "RoundId không tồn tại." });
                return response;
            }

            // Kiểm tra trạng thái Ranking và Round Closed
            var hasRanking = await _unitOfWork.GetRepository<Ranking>().GetFirstOrDefaultAsync(r => r.RoundId == roundId) != null;
            var isClosed = string.Equals(round.Status, RoundConstants.Status.Closed, StringComparison.OrdinalIgnoreCase);

            var scoreRepo = _unitOfWork.GetRepository<ScoreRecord>();
            var criteriaRepo = _unitOfWork.GetRepository<Criterion>();
            var judgeAssignRepo = _unitOfWork.GetRepository<JudgeAssign>();
            var subRepo = _unitOfWork.GetRepository<Submission>();

            // Pre-load data to optimize
            var criteria = await criteriaRepo.GetAllAsync(c => c.RoundId == roundId);
            var criteriaDict = criteria.ToDictionary(c => c.Id);

            var judgeIds = request.Scores.Select(s => s.JudgeId).Distinct().ToList();
            var judgeAssigns = await judgeAssignRepo.GetAllAsync(ja => ja.RoundId == roundId && judgeIds.Contains(ja.JudgeId));
            var assignedJudgeIds = judgeAssigns.Select(ja => ja.JudgeId).ToHashSet();

            var submissionIds = request.Scores.Select(s => s.SubmissionId).Distinct().ToList();
            var submissions = await subRepo.GetAllAsync(s => s.RoundId == roundId && submissionIds.Contains(s.Id));
            var submissionDict = submissions.ToDictionary(s => s.Id);

            var existingScores = await scoreRepo.GetAllAsync(sr => submissionIds.Contains(sr.SubmissionId) && judgeIds.Contains(sr.JudgeId));
            var existingScoresDict = existingScores.ToDictionary(sr => $"{sr.SubmissionId}_{sr.JudgeId}_{sr.CriterionId}");

            foreach (var sReq in request.Scores)
            {
                var rowNumber = sReq.RowNumber;
                try
                {
                    if (hasRanking || isClosed)
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "Không thể import điểm vì Round đã Closed hoặc đã có Ranking." });
                        continue; // Nếu đã có ranking thì thất bại hết, nhưng vẫn chạy hết loop để trả về lý do cho từng row.
                    }

                    // 1. Validate Criterion
                    if (!criteriaDict.TryGetValue(sReq.CriterionId, out var criterion))
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "CriterionId không tồn tại hoặc không thuộc Round này." });
                        continue;
                    }

                    // 2. Validate Judge Assignment
                    if (!assignedJudgeIds.Contains(sReq.JudgeId))
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "Judge không được phân công chấm thi cho Round này." });
                        continue;
                    }

                    // 3. Validate Submission
                    if (!submissionDict.TryGetValue(sReq.SubmissionId, out var submission))
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "SubmissionId không tồn tại hoặc không thuộc Round này." });
                        continue;
                    }

                    if (submission.IsDisqualified)
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = "Không thể chấm điểm cho bài thi đã bị loại." });
                        continue;
                    }

                    // 4. Validate Score Range
                    if (sReq.ScoreValue < 0 || sReq.ScoreValue > criterion.MaxScore)
                    {
                        response.Data.Failed.Add(new BatchImportFailedDto { RowNumber = rowNumber, Reason = $"Điểm số không hợp lệ (0 - {criterion.MaxScore})." });
                        continue;
                    }

                    // 5. Upsert Score
                    var scoreKey = $"{sReq.SubmissionId}_{sReq.JudgeId}_{sReq.CriterionId}";
                    Guid scoreId;

                    if (existingScoresDict.TryGetValue(scoreKey, out var existingScore))
                    {
                        var oldValues = CreateScoreAuditValues(existingScore);

                        existingScore.Score = sReq.ScoreValue;
                        existingScore.Comment = sReq.Note;
                        existingScore.UpdatedAt = now;
                        scoreId = existingScore.Id;
                        
                        scoreRepo.Update(existingScore);

                        await _auditLogService.AddAsync(
                            coordinatorId,
                            AuditActionConstants.ScoreAudit.Update,
                            nameof(ScoreRecord),
                            existingScore.Id.ToString(),
                            oldValues,
                            CreateScoreAuditValues(existingScore));
                    }
                    else
                    {
                        var newScore = new ScoreRecord
                        {
                            Id = Guid.NewGuid(),
                            SubmissionId = sReq.SubmissionId,
                            JudgeId = sReq.JudgeId,
                            CriterionId = sReq.CriterionId,
                            Score = sReq.ScoreValue,
                            Comment = sReq.Note,
                            ScoredAt = now,
                            UpdatedAt = now
                        };
                        await scoreRepo.AddAsync(newScore);
                        scoreId = newScore.Id;
                        
                        // Thêm vào Dictionary để kiểm tra nếu trong cùng mảng import có 2 record trùng nhau
                        existingScoresDict[scoreKey] = newScore;

                        await _auditLogService.AddAsync(
                            coordinatorId,
                            AuditActionConstants.ScoreAudit.Create,
                            nameof(ScoreRecord),
                            newScore.Id.ToString(),
                            newValues: CreateScoreAuditValues(newScore));
                    }

                    await _unitOfWork.SaveChangesAsync();

                    response.Data.Created.Add(new ImportScoreSuccessDto
                    {
                        RowNumber = rowNumber,
                        ScoreId = scoreId,
                        SubmissionId = sReq.SubmissionId,
                        JudgeId = sReq.JudgeId,
                        CriterionId = sReq.CriterionId
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
