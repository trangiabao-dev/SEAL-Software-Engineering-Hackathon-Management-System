using SealHackathon.Application.Common.Calculations;
using SealHackathon.Application.DTOs.TieBreak;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý phiên chấm lại tie-break khi nhiều đội đồng hạng ở vị trí quan trọng.
    /// </summary>
    public class TieBreakService : ITieBreakService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        /// <summary>
        /// Khởi tạo service tie-break với UnitOfWork để gom các thay đổi database vào một lần lưu.
        /// </summary>
        public TieBreakService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Coordinator tạo phiên tie-break cho một hạng đang có nhiều đội đồng hạng.
        /// </summary>
        public async Task<TieBreakSessionResponse> CreateSessionAsync(int roundId, int rankPosition)
        {
            if (rankPosition <= 0)
                throw new BadRequestException(ErrorMessages.TieBreak.InvalidRankPosition);

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(entity => entity.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            EnsureRoundCanUseTieBreak(round);

            var existingSession = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetFirstOrDefaultAsync(session =>
                    session.RoundId == roundId
                    && session.RankPosition == rankPosition
                    && session.Status == TieBreakConstants.Status.PendingScoring);

            if (existingSession is not null)
                throw new ConflictException(ErrorMessages.TieBreak.SessionAlreadyExists);

            var rankings = await _unitOfWork
                .GetRepository<Ranking>()
                .GetAllAsync(ranking =>
                    ranking.RoundId == roundId
                    && ranking.RankPosition == rankPosition
                    && !ranking.Team.IsDeleted
                    && ranking.Team.Status != TeamConstants.Status.Disqualified);

            if (rankings.Count < 2)
                throw new BadRequestException(ErrorMessages.TieBreak.TieBreakGroupNotFound);

            var tiedTeamIds = rankings
                .Select(ranking => ranking.TeamId)
                .Distinct()
                .ToList();

            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllWithIncludeAsync(
                    submission =>
                        submission.RoundId == roundId
                        && tiedTeamIds.Contains(submission.TeamId)
                        && !submission.IsDisqualified
                        && !submission.Team.IsDeleted
                        && submission.Team.Status != TeamConstants.Status.Disqualified,
                    submission => submission.Team);

            // Mỗi đội trong nhóm đồng hạng phải có bài hợp lệ để Judge chấm lại.
            if (submissions.Select(submission => submission.TeamId).Distinct().Count() != tiedTeamIds.Count)
                throw new BadRequestException(ErrorMessages.TieBreak.TieBreakSubmissionMissing);

            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(criterion => criterion.RoundId == roundId);

            if (!criteria.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoCriteria);

            var judgeAssigns = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetAllAsync(assign => assign.RoundId == roundId);

            var judgeIds = judgeAssigns
                .Select(assign => assign.JudgeId)
                .Distinct()
                .ToList();

            if (!judgeIds.Any())
                throw new BadRequestException(ErrorMessages.Ranking.NoAssignedJudges);

            var now = DateTime.UtcNow;
            var session = new TieBreakSession
            {
                Id = Guid.NewGuid(),
                RoundId = roundId,
                RankPosition = rankPosition,
                Status = TieBreakConstants.Status.PendingScoring,
                CreatedAt = now
            };

            await _unitOfWork.GetRepository<TieBreakSession>().AddAsync(session);

            foreach (var submission in submissions)
            {
                await _unitOfWork.GetRepository<TieBreakSubmission>().AddAsync(new TieBreakSubmission
                {
                    Id = Guid.NewGuid(),
                    TieBreakSessionId = session.Id,
                    SubmissionId = submission.Id,
                    CreatedAt = now
                });
            }

            foreach (var judgeId in judgeIds)
            {
                // Notification được add cùng UnitOfWork để phiên tie-break và thông báo cùng thành công hoặc cùng thất bại.
                await _unitOfWork.GetRepository<Notification>().AddAsync(new Notification
                {
                    Id = Guid.NewGuid(),
                    AccountId = judgeId,
                    Title = "Chấm lại tie-break",
                    Message = $"Bạn cần chấm lại nhóm đồng hạng hạng {rankPosition} của vòng thi {round.Name}.",
                    Type = TieBreakConstants.NotificationType.Required,
                    IsRead = false,
                    CreatedAt = now
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return await BuildSessionResponseAsync(session.Id);
        }

        /// <summary>
        /// Tự tạo phiên tie-break nếu chưa có phiên đang chờ chấm cho cùng Round và cùng hạng.
        /// </summary>
        public async Task<TieBreakSessionResponse?> CreateSessionIfNotExistsAsync(
            int roundId,
            int rankPosition)
        {
            if (rankPosition <= 0)
                throw new BadRequestException(ErrorMessages.TieBreak.InvalidRankPosition);

            var existingSession = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetFirstOrDefaultAsync(session =>
                    session.RoundId == roundId
                    && session.RankPosition == rankPosition
                    && session.Status == TieBreakConstants.Status.PendingScoring);

            // Ranking có thể được tính lại nhiều lần; nếu phiên đang chờ đã tồn tại thì không tạo trùng.
            if (existingSession is not null)
                return await BuildSessionResponseAsync(existingSession.Id);

            return await CreateSessionAsync(roundId, rankPosition);
        }

        /// <summary>
        /// Judge lấy các phiên tie-break đang chờ chấm thuộc Round mình được phân công.
        /// </summary>
        public async Task<List<TieBreakSessionResponse>> GetMyPendingSessionsAsync(Guid judgeId)
        {
            var judgeAssigns = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetAllAsync(assign => assign.JudgeId == judgeId);

            var roundIds = judgeAssigns
                .Select(assign => assign.RoundId)
                .Distinct()
                .ToList();

            if (!roundIds.Any())
                return new List<TieBreakSessionResponse>();

            var sessions = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetAllAsync(session =>
                    roundIds.Contains(session.RoundId)
                    && session.Status == TieBreakConstants.Status.PendingScoring);

            return await BuildSessionResponsesAsync(sessions);
        }

        /// <summary>
        /// Coordinator lấy danh sách toàn bộ các phiên tie-break của một Round.
        /// </summary>
        public async Task<List<TieBreakSessionResponse>> GetSessionsByRoundAsync(int roundId)
        {
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            var sessions = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetAllAsync(session => session.RoundId == roundId);

            return await BuildSessionResponsesAsync(sessions);
        }

        /// <summary>
        /// Lấy chi tiết phiên tie-break, có kiểm tra quyền xem của Judge.
        /// </summary>
        public async Task<TieBreakSessionResponse> GetSessionAsync(
            Guid sessionId,
            Guid currentAccountId,
            bool isCoordinator)
        {
            var session = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetFirstOrDefaultAsync(entity => entity.Id == sessionId);

            if (session is null)
                throw new NotFoundException(ErrorMessages.TieBreak.SessionNotFound);

            if (!isCoordinator)
                await EnsureJudgeAssignedToSessionRoundAsync(currentAccountId, session.RoundId);

            return await BuildSessionResponseAsync(session.Id);
        }

        /// <summary>
        /// Lấy danh sách điểm tie-break của một bài trong phiên chấm lại.
        /// </summary>
        public async Task<List<TieBreakScoreResponse>> GetScoresByTieBreakSubmissionAsync(
            Guid tieBreakSubmissionId,
            Guid currentAccountId,
            bool isCoordinator)
        {
            var context = await GetTieBreakSubmissionContextAsync(tieBreakSubmissionId);

            if (!isCoordinator)
                await EnsureJudgeAssignedToSessionRoundAsync(currentAccountId, context.Session.RoundId);

            var scoreRecords = await _unitOfWork
                .GetRepository<TieBreakScoreRecord>()
                .GetAllWithIncludeAsync(
                    scoreRecord =>
                        scoreRecord.TieBreakSubmissionId == tieBreakSubmissionId
                        && (isCoordinator || scoreRecord.JudgeId == currentAccountId),
                    scoreRecord => scoreRecord.Criterion,
                    scoreRecord => scoreRecord.Judge);

            return scoreRecords
                .OrderBy(scoreRecord => scoreRecord.CriterionId)
                .Select(MapToTieBreakScoreResponse)
                .ToList();
        }

        /// <summary>
        /// Judge chấm một tiêu chí cho một bài trong phiên tie-break.
        /// </summary>
        public async Task<TieBreakScoreResponse> SubmitScoreAsync(
            Guid tieBreakSubmissionId,
            Guid judgeId,
            SubmitTieBreakScoreRequest request)
        {
            var context = await GetTieBreakSubmissionContextAsync(tieBreakSubmissionId);

            EnsureTieBreakSessionPending(context.Session);
            EnsureRoundCanUseTieBreak(context.Round);
            await EnsureJudgeAssignedToSessionRoundAsync(judgeId, context.Session.RoundId);

            var criterion = await GetCriterionForSessionRoundAsync(
                request.CriterionId,
                context.Session.RoundId);

            EnsureTieBreakScoreInRange(request.Score, criterion);

            var existingScore = await _unitOfWork
                .GetRepository<TieBreakScoreRecord>()
                .GetFirstOrDefaultAsync(scoreRecord =>
                    scoreRecord.TieBreakSubmissionId == tieBreakSubmissionId
                    && scoreRecord.JudgeId == judgeId
                    && scoreRecord.CriterionId == request.CriterionId);

            if (existingScore is not null)
                throw new ConflictException(ErrorMessages.TieBreak.AlreadyScored);

            var scoreRecord = new TieBreakScoreRecord
            {
                Id = Guid.NewGuid(),
                TieBreakSubmissionId = tieBreakSubmissionId,
                JudgeId = judgeId,
                CriterionId = request.CriterionId,
                Score = request.Score,
                Comment = request.Comment,
                ScoredAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<TieBreakScoreRecord>().AddAsync(scoreRecord);

            // Lưu audit cùng lần SaveChanges để điểm tie-break và lịch sử thay đổi luôn nhất quán.
            await _auditLogService.AddAsync(
                judgeId,
                AuditActionConstants.TieBreakScoreAudit.Create,
                nameof(TieBreakScoreRecord),
                scoreRecord.Id.ToString(),
                newValues: CreateTieBreakScoreAuditValues(
                    scoreRecord,
                    context.Session.Id,
                    context.Submission.Id));

            await _unitOfWork.SaveChangesAsync();

            return MapToTieBreakScoreResponse(scoreRecord, criterion.Name);
        }

        /// <summary>
        /// Judge sửa điểm tie-break do chính mình đã chấm.
        /// </summary>
        public async Task<TieBreakScoreResponse> UpdateScoreAsync(
            Guid tieBreakScoreRecordId,
            Guid judgeId,
            UpdateTieBreakScoreRequest request)
        {
            var scoreRecord = await _unitOfWork
                .GetRepository<TieBreakScoreRecord>()
                .GetFirstOrDefaultAsync(record => record.Id == tieBreakScoreRecordId);

            if (scoreRecord is null)
                throw new NotFoundException(ErrorMessages.TieBreak.ScoreRecordNotFound);

            if (scoreRecord.JudgeId != judgeId)
                throw new ForbiddenException(ErrorMessages.TieBreak.JudgeNoUpdatePermission);

            var context = await GetTieBreakSubmissionContextAsync(scoreRecord.TieBreakSubmissionId);

            EnsureTieBreakSessionPending(context.Session);
            EnsureRoundCanUseTieBreak(context.Round);
            await EnsureJudgeAssignedToSessionRoundAsync(judgeId, context.Session.RoundId);

            var criterion = await GetCriterionForSessionRoundAsync(
                scoreRecord.CriterionId,
                context.Session.RoundId);

            EnsureTieBreakScoreInRange(request.UpdatedScore, criterion);

            var oldValues = CreateTieBreakScoreAuditValues(
                scoreRecord,
                context.Session.Id,
                context.Submission.Id);

            scoreRecord.Score = request.UpdatedScore;
            scoreRecord.Comment = request.UpdatedComment;
            scoreRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<TieBreakScoreRecord>().Update(scoreRecord);

            // Audit cần lưu cả trạng thái cũ và mới để Coordinator truy vết Judge đã sửa gì.
            await _auditLogService.AddAsync(
                judgeId,
                AuditActionConstants.TieBreakScoreAudit.Update,
                nameof(TieBreakScoreRecord),
                scoreRecord.Id.ToString(),
                oldValues,
                CreateTieBreakScoreAuditValues(
                    scoreRecord,
                    context.Session.Id,
                    context.Submission.Id));

            await _unitOfWork.SaveChangesAsync();

            return MapToTieBreakScoreResponse(scoreRecord, criterion.Name);
        }

        /// <summary>
        /// Coordinator tính kết quả tie-break và cập nhật thứ hạng chính thức trong bảng Ranking.
        /// </summary>
        public async Task<TieBreakSessionResponse> CalculateResultAsync(Guid sessionId)
        {
            var session = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetFirstOrDefaultAsync(entity => entity.Id == sessionId);

            if (session is null)
                throw new NotFoundException(ErrorMessages.TieBreak.SessionNotFound);

            EnsureTieBreakSessionPending(session);

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(entity => entity.Id == session.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            EnsureRoundCanUseTieBreak(round);

            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(criterion => criterion.RoundId == session.RoundId);

            if (!criteria.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoCriteria);

            var judgeAssigns = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetAllAsync(assign => assign.RoundId == session.RoundId);

            var judgeIds = judgeAssigns
                .Select(assign => assign.JudgeId)
                .Distinct()
                .ToList();

            if (!judgeIds.Any())
                throw new BadRequestException(ErrorMessages.Ranking.NoAssignedJudges);

            var tieBreakSubmissions = await _unitOfWork
                .GetRepository<TieBreakSubmission>()
                .GetAllAsync(item => item.TieBreakSessionId == session.Id);

            if (tieBreakSubmissions.Count < 2)
                throw new BadRequestException(ErrorMessages.TieBreak.TieBreakGroupNotFound);

            var submissionIds = tieBreakSubmissions
                .Select(item => item.SubmissionId)
                .Distinct()
                .ToList();

            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllWithIncludeAsync(
                    submission =>
                        submissionIds.Contains(submission.Id)
                        && !submission.IsDisqualified
                        && !submission.Team.IsDeleted
                        && submission.Team.Status != TeamConstants.Status.Disqualified,
                    submission => submission.Team);

            // Nếu thiếu bài hợp lệ thì không thể tính tie-break vì Judge đã chấm lại trên dữ liệu không còn hợp lệ.
            if (submissions.Count != tieBreakSubmissions.Count)
                throw new BadRequestException(ErrorMessages.TieBreak.TieBreakSubmissionMissing);

            var tieBreakSubmissionIds = tieBreakSubmissions
                .Select(item => item.Id)
                .ToList();

            var scoreRecords = await _unitOfWork
                .GetRepository<TieBreakScoreRecord>()
                .GetAllAsync(scoreRecord => tieBreakSubmissionIds.Contains(scoreRecord.TieBreakSubmissionId));

            EnsureAllRequiredTieBreakScoresExist(
                tieBreakSubmissions,
                criteria,
                judgeIds,
                scoreRecords);

            var orderedTeamIds = CalculateTieBreakTeamOrder(
                tieBreakSubmissions,
                submissions,
                criteria,
                scoreRecords);

            await ApplyTieBreakRankingOrderAsync(session, round, orderedTeamIds);

            session.Status = TieBreakConstants.Status.Completed;
            session.CompletedAt = DateTime.UtcNow;

            _unitOfWork.GetRepository<TieBreakSession>().Update(session);
            await _unitOfWork.SaveChangesAsync();

            return await BuildSessionResponseAsync(session.Id);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Đảm bảo mọi Judge được assign đã chấm mọi Criterion cho mọi bài trong phiên tie-break.
        /// </summary>
        private static void EnsureAllRequiredTieBreakScoresExist(
            IReadOnlyCollection<TieBreakSubmission> tieBreakSubmissions,
            IReadOnlyCollection<Criterion> criteria,
            IReadOnlyCollection<Guid> judgeIds,
            IReadOnlyCollection<TieBreakScoreRecord> scoreRecords)
        {
            var scoredKeys = scoreRecords
                .Select(scoreRecord => (
                    scoreRecord.TieBreakSubmissionId,
                    scoreRecord.JudgeId,
                    scoreRecord.CriterionId))
                .ToHashSet();

            foreach (var tieBreakSubmission in tieBreakSubmissions)
            {
                foreach (var judgeId in judgeIds)
                {
                    foreach (var criterion in criteria)
                    {
                        if (!scoredKeys.Contains((tieBreakSubmission.Id, judgeId, criterion.Id)))
                            throw new BadRequestException(ErrorMessages.TieBreak.ScoresNotCompleted);
                    }
                }
            }
        }

        /// <summary>
        /// Tính điểm tie-break và trả về thứ tự TeamId từ cao xuống thấp.
        /// </summary>
        private static List<Guid> CalculateTieBreakTeamOrder(
            IReadOnlyCollection<TieBreakSubmission> tieBreakSubmissions,
            IReadOnlyCollection<Submission> submissions,
            IReadOnlyCollection<Criterion> criteria,
            IReadOnlyCollection<TieBreakScoreRecord> scoreRecords)
        {
            var criterionConfigById = criteria.ToDictionary(
                criterion => criterion.Id,
                criterion => new
                {
                    criterion.MaxScore,
                    criterion.Weight
                });

            var submissionById = submissions.ToDictionary(submission => submission.Id);
            var submissionIdByTieBreakSubmissionId = tieBreakSubmissions.ToDictionary(
                item => item.Id,
                item => item.SubmissionId);

            var teamScores = scoreRecords
                .GroupBy(scoreRecord => scoreRecord.TieBreakSubmissionId)
                .Select(group =>
                {
                    var submissionId = submissionIdByTieBreakSubmissionId[group.Key];
                    var submission = submissionById[submissionId];

                    var totalScore = group
                        .GroupBy(scoreRecord => scoreRecord.CriterionId)
                        .Sum(criterionGroup =>
                        {
                            if (!criterionConfigById.TryGetValue(criterionGroup.Key, out var criterionConfig))
                                throw new BadRequestException(ErrorMessages.Ranking.CriterionConfigNotFound);

                            var averageScore = criterionGroup.Average(scoreRecord => scoreRecord.Score);

                            return ScoreCalculation.CalculateWeightedCriterionScore(
                                averageScore,
                                criterionConfig.MaxScore,
                                criterionConfig.Weight);
                        });

                    return (
                        submission.TeamId,
                        TotalScore: Math.Round(totalScore, 4));
                })
                .ToList();

            EnsureTieBreakResultHasNoTie(teamScores);

            return teamScores
                .OrderByDescending(teamScore => teamScore.TotalScore)
                .Select(teamScore => teamScore.TeamId)
                .ToList();
        }

        /// <summary>
        /// Chặn kết quả tie-break vẫn còn đồng điểm để tránh backend tự chọn đội thắng sai nghiệp vụ.
        /// </summary>
        private static void EnsureTieBreakResultHasNoTie(
            IReadOnlyCollection<(Guid TeamId, double TotalScore)> teamScores)
        {
            var stillTied = teamScores
                .GroupBy(teamScore => teamScore.TotalScore)
                .Any(group => group.Count() > 1);

            if (stillTied)
                throw new BadRequestException(ErrorMessages.TieBreak.ResultStillTied);
        }

        /// <summary>
        /// Áp thứ tự tie-break vào bảng Ranking và tính lại cờ IsAdvancing.
        /// </summary>
        private async Task ApplyTieBreakRankingOrderAsync(
            TieBreakSession session,
            Round round,
            IReadOnlyList<Guid> orderedTeamIds)
        {
            if (orderedTeamIds.Count < 2)
                throw new BadRequestException(ErrorMessages.Ranking.TieBreakRequiresMultipleTeams);

            if (orderedTeamIds.Distinct().Count() != orderedTeamIds.Count)
                throw new BadRequestException(ErrorMessages.Ranking.TieBreakTeamDuplicated);

            var rankingRepository = _unitOfWork.GetRepository<Ranking>();
            var rankings = await rankingRepository
                .GetAllAsync(ranking => ranking.RoundId == session.RoundId);

            if (!rankings.Any())
                throw new NotFoundException(ErrorMessages.Ranking.NotFound);

            var orderedTeamIdSet = orderedTeamIds.ToHashSet();
            var selectedRankings = rankings
                .Where(ranking => orderedTeamIdSet.Contains(ranking.TeamId))
                .ToList();

            if (selectedRankings.Count != orderedTeamIds.Count)
                throw new BadRequestException(ErrorMessages.Ranking.TieBreakTeamNotInRanking);

            if (selectedRankings.Any(ranking => ranking.RankPosition != session.RankPosition))
                throw new BadRequestException(ErrorMessages.Ranking.TieBreakTeamsNotSameRank);

            var completeTieTeamIdSet = rankings
                .Where(ranking => ranking.RankPosition == session.RankPosition)
                .Select(ranking => ranking.TeamId)
                .ToHashSet();

            // Phiên tie-break phải xử lý đủ cả nhóm đồng hạng, không chỉ vài đội trong nhóm.
            if (!completeTieTeamIdSet.SetEquals(orderedTeamIdSet))
                throw new BadRequestException(ErrorMessages.Ranking.TieBreakGroupIncomplete);

            var rankingByTeamId = selectedRankings.ToDictionary(ranking => ranking.TeamId);
            for (var index = 0; index < orderedTeamIds.Count; index++)
            {
                var teamId = orderedTeamIds[index];
                rankingByTeamId[teamId].RankPosition = session.RankPosition + index;
            }

            var orderedRankings = rankings
                .OrderBy(ranking => ranking.RankPosition)
                .Select(ranking => (
                    ranking.TeamId,
                    ranking.TotalScore,
                    Rank: ranking.RankPosition))
                .ToList();

            var unresolvedAdvancingRank = GetUnresolvedAdvancingRank(
                orderedRankings,
                round.AdvancingSlots);

            foreach (var ranking in rankings)
            {
                ranking.IsAdvancing = round.AdvancingSlots.HasValue
                    && !unresolvedAdvancingRank.HasValue
                    && ranking.RankPosition <= round.AdvancingSlots.Value;

                rankingRepository.Update(ranking);
            }
        }

        /// <summary>
        /// Tìm hạng còn đồng hạng đúng tại ranh giới chọn đội đi tiếp.
        /// </summary>
        private static int? GetUnresolvedAdvancingRank(
            IReadOnlyList<(Guid TeamId, double TotalScore, int Rank)> rankedTeams,
            int? advancingSlots)
        {
            if (!advancingSlots.HasValue)
                return null;

            var cutoff = advancingSlots.Value;

            if (cutoff <= 0 || cutoff >= rankedTeams.Count)
                return null;

            var lastSelectedTeam = rankedTeams[cutoff - 1];
            var firstExcludedTeam = rankedTeams[cutoff];

            return lastSelectedTeam.Rank == firstExcludedTeam.Rank
                ? lastSelectedTeam.Rank
                : null;
        }

        /// <summary>
        /// Chỉ cho tạo tie-break khi Round còn ở Scoring để Judge vẫn có thể chấm lại.
        /// </summary>
        private static void EnsureRoundCanUseTieBreak(Round round)
        {
            if (!string.Equals(round.Status, RoundConstants.Status.Scoring, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.TieBreak.RoundNotInScoring);
        }

        /// <summary>
        /// Chỉ cho Judge chấm hoặc sửa khi phiên tie-break còn chờ chấm.
        /// </summary>
        private static void EnsureTieBreakSessionPending(TieBreakSession session)
        {
            if (!string.Equals(
                    session.Status,
                    TieBreakConstants.Status.PendingScoring,
                    StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.TieBreak.SessionNotPending);
        }

        /// <summary>
        /// Kiểm tra điểm tie-break không âm và không vượt điểm tối đa của tiêu chí.
        /// </summary>
        private static void EnsureTieBreakScoreInRange(double score, Criterion criterion)
        {
            if (score < 0 || score > criterion.MaxScore)
                throw new BadRequestException(ErrorMessages.TieBreak.InvalidScoreRange);
        }

        /// <summary>
        /// Kiểm tra Judge có được phân công vào Round của phiên tie-break hay không.
        /// </summary>
        private async Task EnsureJudgeAssignedToSessionRoundAsync(Guid judgeId, int roundId)
        {
            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(assign =>
                    assign.JudgeId == judgeId
                    && assign.RoundId == roundId);

            if (judgeAssign is null)
                throw new ForbiddenException(ErrorMessages.TieBreak.JudgeNotAssignedToSession);
        }

        /// <summary>
        /// Lấy đủ dữ liệu nền của một TieBreakSubmission để các hàm chấm/sửa không query trùng logic.
        /// </summary>
        private async Task<(
            TieBreakSubmission TieBreakSubmission,
            TieBreakSession Session,
            Submission Submission,
            Round Round)> GetTieBreakSubmissionContextAsync(Guid tieBreakSubmissionId)
        {
            var tieBreakSubmission = await _unitOfWork
                .GetRepository<TieBreakSubmission>()
                .GetFirstOrDefaultAsync(entity => entity.Id == tieBreakSubmissionId);

            if (tieBreakSubmission is null)
                throw new NotFoundException(ErrorMessages.TieBreak.SubmissionNotInSession);

            var session = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetFirstOrDefaultAsync(entity => entity.Id == tieBreakSubmission.TieBreakSessionId);

            if (session is null)
                throw new NotFoundException(ErrorMessages.TieBreak.SessionNotFound);

            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllWithIncludeAsync(
                    submission => submission.Id == tieBreakSubmission.SubmissionId,
                    submission => submission.Team);

            var submission = submissions.FirstOrDefault();

            if (submission is null)
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            // Nếu bài hoặc đội đã bị loại sau khi tạo tie-break thì không tiếp tục chấm lại.
            if (submission.IsDisqualified)
                throw new BadRequestException(ErrorMessages.Score.SubmissionDisqualified);

            if (submission.Team.IsDeleted
                || string.Equals(
                    submission.Team.Status,
                    TeamConstants.Status.Disqualified,
                    StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.Score.TeamDisqualified);

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(entity => entity.Id == session.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            return (tieBreakSubmission, session, submission, round);
        }

        /// <summary>
        /// Lấy tiêu chí và đảm bảo tiêu chí thuộc đúng Round của phiên tie-break.
        /// </summary>
        private async Task<Criterion> GetCriterionForSessionRoundAsync(int criterionId, int roundId)
        {
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(entity => entity.Id == criterionId);

            if (criterion is null)
                throw new NotFoundException(ErrorMessages.Score.CriterionNotFound);

            if (criterion.RoundId != roundId)
                throw new BadRequestException(ErrorMessages.TieBreak.CriterionNotInSessionRound);

            return criterion;
        }

        /// <summary>
        /// Build response cho một phiên tie-break.
        /// </summary>
        private async Task<TieBreakSessionResponse> BuildSessionResponseAsync(Guid sessionId)
        {
            var sessions = await _unitOfWork
                .GetRepository<TieBreakSession>()
                .GetAllAsync(session => session.Id == sessionId);

            if (!sessions.Any())
                throw new NotFoundException(ErrorMessages.TieBreak.SessionNotFound);

            var responses = await BuildSessionResponsesAsync(sessions);
            return responses[0];
        }

        /// <summary>
        /// Build response theo batch để tránh query từng dòng TieBreakSubmission riêng lẻ.
        /// </summary>
        private async Task<List<TieBreakSessionResponse>> BuildSessionResponsesAsync(
            List<TieBreakSession> sessions)
        {
            if (!sessions.Any())
                return new List<TieBreakSessionResponse>();

            var roundIds = sessions
                .Select(session => session.RoundId)
                .Distinct()
                .ToList();

            var rounds = await _unitOfWork
                .GetRepository<Round>()
                .GetAllAsync(round => roundIds.Contains(round.Id));

            var roundById = rounds.ToDictionary(round => round.Id);

            var sessionIds = sessions
                .Select(session => session.Id)
                .ToList();

            var tieBreakSubmissions = await _unitOfWork
                .GetRepository<TieBreakSubmission>()
                .GetAllAsync(item => sessionIds.Contains(item.TieBreakSessionId));

            var submissionIds = tieBreakSubmissions
                .Select(item => item.SubmissionId)
                .Distinct()
                .ToList();

            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllWithIncludeAsync(
                    submission => submissionIds.Contains(submission.Id),
                    submission => submission.Team);

            var submissionById = submissions.ToDictionary(submission => submission.Id);

            var submissionsBySessionId = tieBreakSubmissions
                .GroupBy(item => item.TieBreakSessionId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var allCriteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(criterion => roundIds.Contains(criterion.RoundId));

            var criteriaByRoundId = allCriteria
                .GroupBy(criterion => criterion.RoundId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(c => new SealHackathon.Application.DTOs.Criteria.CriterionResponse
                    {
                        Id = c.Id,
                        RoundId = c.RoundId,
                        Name = c.Name,
                        Description = c.Description,
                        MaxScore = c.MaxScore,
                        Weight = c.Weight
                    }).ToList());

            return sessions
                .OrderByDescending(session => session.CreatedAt)
                .Select(session =>
                {
                    var roundName = roundById.TryGetValue(session.RoundId, out var round)
                        ? round.Name
                        : string.Empty;

                    var items = submissionsBySessionId.TryGetValue(session.Id, out var sessionSubmissions)
                        ? sessionSubmissions
                        : new List<TieBreakSubmission>();

                    var criteria = criteriaByRoundId.TryGetValue(session.RoundId, out var roundCriteria)
                        ? roundCriteria
                        : new List<SealHackathon.Application.DTOs.Criteria.CriterionResponse>();

                    return new TieBreakSessionResponse
                    {
                        Id = session.Id,
                        RoundId = session.RoundId,
                        RoundName = roundName,
                        RankPosition = session.RankPosition,
                        Status = session.Status,
                        CreatedAt = session.CreatedAt,
                        CompletedAt = session.CompletedAt,
                        Submissions = items
                            .Select(item => MapSubmissionResponse(item, submissionById))
                            .ToList(),
                        Criteria = criteria
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Chuyển điểm tie-break sang DTO trả về cho FE.
        /// </summary>
        private static TieBreakScoreResponse MapToTieBreakScoreResponse(
            TieBreakScoreRecord scoreRecord)
        {
            return MapToTieBreakScoreResponse(
                scoreRecord,
                scoreRecord.Criterion?.Name ?? string.Empty,
                scoreRecord.Judge?.Username ?? string.Empty);
        }

        /// <summary>
        /// Chuyển điểm tie-break sang DTO khi service đã có sẵn tên Criterion hoặc Judge.
        /// </summary>
        private static TieBreakScoreResponse MapToTieBreakScoreResponse(
            TieBreakScoreRecord scoreRecord,
            string criterionName,
            string judgeName = "")
        {
            return new TieBreakScoreResponse
            {
                Id = scoreRecord.Id,
                TieBreakSubmissionId = scoreRecord.TieBreakSubmissionId,
                JudgeId = scoreRecord.JudgeId,
                JudgeName = judgeName,
                CriterionId = scoreRecord.CriterionId,
                CriterionName = criterionName,
                Score = scoreRecord.Score,
                Comment = scoreRecord.Comment,
                ScoredAt = scoreRecord.ScoredAt,
                UpdatedAt = scoreRecord.UpdatedAt
            };
        }

        /// <summary>
        /// Tạo bản chụp điểm tie-break để AuditLog lưu được trạng thái trước và sau khi Judge chấm/sửa.
        /// </summary>
        private static object CreateTieBreakScoreAuditValues(
            TieBreakScoreRecord scoreRecord,
            Guid tieBreakSessionId,
            Guid submissionId)
        {
            return new
            {
                scoreRecord.TieBreakSubmissionId,
                TieBreakSessionId = tieBreakSessionId,
                SubmissionId = submissionId,
                scoreRecord.JudgeId,
                scoreRecord.CriterionId,
                scoreRecord.Score,
                scoreRecord.Comment
            };
        }

        private static TieBreakSubmissionResponse MapSubmissionResponse(
            TieBreakSubmission tieBreakSubmission,
            IReadOnlyDictionary<Guid, Submission> submissionById)
        {
            if (!submissionById.TryGetValue(tieBreakSubmission.SubmissionId, out var submission))
                throw new NotFoundException(ErrorMessages.Submission.NotFound);

            return new TieBreakSubmissionResponse
            {
                TieBreakSubmissionId = tieBreakSubmission.Id,
                SubmissionId = submission.Id,
                TeamId = submission.TeamId,
                TeamName = submission.Team.TeamName,
                University = submission.Team.University,
                PresentationUrl = submission.PresentationUrl
            };
        }
    }
}
