using SealHackathon.Application.Common.Calculations;
using SealHackathon.Application.DTOs.Ranking;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý nghiệp vụ xếp hạng: tính tổng điểm có trọng số, xếp hạng và xác định đội được vào vòng tiếp theo.
    /// </summary>
    public class RankingService : IRankingService
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> RoundRankingLocks = new();

        private readonly IUnitOfWork _unitOfWork;

        public RankingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tính hoặc tính lại bảng xếp hạng cho một Round, sau đó lưu kết quả xuống database.
        /// </summary>
        public Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId)
        {
            return ExecuteWithRoundRankingLockAsync(
                roundId,
                () => CalculateRankingCoreAsync(roundId));
        }

        /// <summary>
        /// Lưu thứ tự chính thức của nhóm đồng hạng sau khi Judge xét tiêu chí phụ.
        /// </summary>
        public Task<RankingLeaderboardResponse> ResolveTieAsync(
            int roundId,
            ResolveRankingTieRequest request)
        {
            return ExecuteWithRoundRankingLockAsync(
                roundId,
                () => ResolveTieCoreAsync(roundId, request));
        }

        /// <summary>
        /// Chạy logic tính ranking thật sự sau khi đã giữ khóa theo Round.
        /// </summary>
        private async Task<RankingLeaderboardResponse> CalculateRankingCoreAsync(int roundId)
        {
            // Bước 1: Kiểm tra Round có tồn tại.
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            EnsureRoundCanCalculateRanking(round);

            // Bước 2: Lấy tiêu chí của Round để biết trọng số điểm.
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => c.RoundId == roundId);

            if (!criteria.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoCriteria);

            // Weight được lưu theo thang 0-1; Ranking chỉ được tính khi tổng bằng 100%.
            var totalWeight = criteria.Sum(criterion => criterion.Weight);

            // Dùng độ lệch nhỏ để tránh báo lỗi do sai số của kiểu double.
            // Tổng Weight phải bằng 1.0, tương đương 100%.
            // Math.Abs tính độ lệch tuyệt đối giữa tổng Weight và 1.0.
            // Nếu độ lệch lớn hơn 0.0001, tổng Weight không hợp lệ.
            // Khi đó hệ thống trả lỗi 400 và dừng tính Ranking.
            // Nếu độ lệch bằng 0 hoặc rất nhỏ, hệ thống tiếp tục tính Ranking.
            if (Math.Abs(totalWeight - 1.0) > 0.0001)
                throw new BadRequestException(ErrorMessages.Ranking.CriteriaWeightTotalInvalid);

            var criterionScoreConfigDict = criteria.ToDictionary(
                c => c.Id,
                c => new
                {
                    c.Weight,
                    c.MaxScore
                });

            // Bước 3: Lấy các bài nộp hợp lệ của Round.
            // Bài nộp hợp lệ là bài chưa bị loại và team cũng chưa bị loại/xóa.
            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllAsync(s => s.RoundId == roundId
                                  && !s.IsDisqualified
                                  && !s.Team.IsDeleted
                                  && s.Team.Status != TeamConstants.Status.Disqualified);

            if (!submissions.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoValidSubmissions);

            var submissionIds = submissions.Select(s => s.Id).ToList();
            var submissionTeamDict = submissions.ToDictionary(s => s.Id, s => s.TeamId);

            // Dùng lần nộp cuối để tránh đội tạo bài tạm sớm nhằm chiếm ưu thế tie-break.
            var submittedAtByTeamId = submissions.ToDictionary(
                submission => submission.TeamId,
                submission => submission.UpdatedAt ?? submission.CreatedAt);

            // Ranking chỉ tính điểm từ judge được assign và còn quyền Judge hợp lệ trong Event.
            var assignedJudgeIds = await GetActiveAssignedJudgeIdsAsync(round);

            // Bước 4: Lấy ScoreRecord hợp lệ, bỏ qua điểm hiệu chuẩn và điểm của judge không được assign.
            var allScoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(scoreRecord => submissionIds.Contains(scoreRecord.SubmissionId)
                                && assignedJudgeIds.Contains(scoreRecord.JudgeId));

            // Backend vẫn phải kiểm đủ điểm để tránh FE lỗi hoặc API bị gọi trực tiếp.
            EnsureAllRequiredScoresExist(
                submissions,
                criteria,
                assignedJudgeIds,
                allScoreRecords);

            // Bước 5: Tính TotalScore cho từng team.
            // Công thức: TotalScore = tổng của ((điểm trung bình / điểm tối đa tiêu chí) x trọng số tiêu chí x 100).
            var teamScores = allScoreRecords
                .GroupBy(sr => submissionTeamDict[sr.SubmissionId])
                .Select(teamGroup => new
                {
                    TeamId = teamGroup.Key,
                    TotalScore = teamGroup
                        .GroupBy(sr => sr.CriterionId)
                        .Sum(criterionGroup =>
                        {
                            if (!criterionScoreConfigDict.TryGetValue(criterionGroup.Key, out var criterionConfig))
                            {
                                throw new BadRequestException(ErrorMessages.Ranking.CriterionConfigNotFound);
                            }

                            if (criterionConfig.MaxScore <= 0)
                            {
                                throw new BadRequestException(ErrorMessages.Ranking.CriterionMaxScoreInvalid);
                            }

                            var avgScore = criterionGroup.Average(sr => sr.Score);

                            return ScoreCalculation.CalculateWeightedCriterionScore(
                                avgScore,
                                criterionConfig.MaxScore,
                                criterionConfig.Weight);
                        })
                })
                .ToList();

            // Bước 5b: Thêm các team có Submission nhưng chưa có điểm (TotalScore = 0)
            var teamIdsWithScores = teamScores.Select(ts => ts.TeamId).ToHashSet();
            var teamIdsWithoutScores = submissions
                .Select(s => s.TeamId)
                .Distinct()
                .Where(tid => !teamIdsWithScores.Contains(tid))
                .ToList();

            // Làm tròn trước khi so sánh để điểm xếp hạng khớp điểm lưu trong database.
            var allTeamScores = teamScores
                .Select(teamScore => (
                    TeamId: teamScore.TeamId,
                    TotalScore: Math.Round(teamScore.TotalScore, 4),
                    SubmittedAt: submittedAtByTeamId[teamScore.TeamId]))
                .Concat(teamIdsWithoutScores.Select(teamId => (
                    TeamId: teamId,
                    TotalScore: 0.0,
                    SubmittedAt: submittedAtByTeamId[teamId])))
                .OrderByDescending(team => team.TotalScore)
                .ThenBy(team => team.SubmittedAt)
                .ToList();

            // Bước 6: Xếp điểm giảm dần và dùng thời gian nộp cuối làm tie-break tự động.
            var rankedTeams = new List<(Guid TeamId, double TotalScore, int Rank)>();
            for (var index = 0; index < allTeamScores.Count; index++)
            {
                var rank = index + 1;

                if (index > 0)
                {
                    var currentTeam = allTeamScores[index];
                    var previousTeam = allTeamScores[index - 1];

                    // Chỉ giữ đồng hạng khi cả điểm và thời gian nộp cuối đều bằng nhau.
                    if (currentTeam.TotalScore == previousTeam.TotalScore
                        && currentTeam.SubmittedAt == previousTeam.SubmittedAt)
                    {
                        rank = rankedTeams[index - 1].Rank;
                    }
                }

                rankedTeams.Add((
                    allTeamScores[index].TeamId,
                    allTeamScores[index].TotalScore,
                    rank));
            }

            // Bước 7: Xác định đội được vào vòng tiếp theo.
            // AdvancingSlots null đánh dấu vòng chung kết nên không có đội đi tiếp.
            var advancingSlots = round.AdvancingSlots;
            var unresolvedAdvancingRank = GetUnresolvedAdvancingRank(
                rankedTeams,
                advancingSlots);

            var now = DateTime.UtcNow;

            // Bước 8: Xóa ranking cũ và thêm ranking mới.
            var existingRankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(r => r.RoundId == roundId);

            foreach (var old in existingRankings)
            {
                _unitOfWork.GetRepository<Domain.Entities.Ranking>().Delete(old);
            }

            var newRankings = new List<Domain.Entities.Ranking>();
            foreach (var team in rankedTeams)
            {
                // Khi tie tại cutoff chưa được xử lý, chưa đội nào được đi tiếp để RoundService chặn mở vòng sau.
                var isAdvancing = advancingSlots.HasValue
                    && !unresolvedAdvancingRank.HasValue
                    && team.Rank <= advancingSlots.Value;

                var ranking = new Domain.Entities.Ranking
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    RoundId = roundId,
                    TotalScore = team.TotalScore,
                    RankPosition = team.Rank,
                    IsAdvancing = isAdvancing,
                    CalculatedAt = now
                };

                newRankings.Add(ranking);
                await _unitOfWork.GetRepository<Domain.Entities.Ranking>().AddAsync(ranking);
            }

            await _unitOfWork.SaveChangesAsync();

            // Bước 9: Chuyển dữ liệu sang response DTO.
            var teamIds = rankedTeams.Select(r => r.TeamId).ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));
            var teamNameDict = teams.ToDictionary(t => t.Id, t => t.TeamName);

            var rankingResponses = newRankings
                .OrderBy(r => r.RankPosition)
                .Select(r => MapToRankingResponse(
                    r, round.Name, teamNameDict.GetValueOrDefault(r.TeamId, string.Empty)))
                .ToList();

            return new RankingLeaderboardResponse
            {
                RoundId = round.Id,
                RoundName = round.Name,
                AdvancingSlots = round.AdvancingSlots,
                TotalTeams = rankingResponses.Count,
                CalculatedAt = now,
                Rankings = rankingResponses
            };
        }

        /// <summary>
        /// Cập nhật RankPosition và IsAdvancing của nhóm đồng hạng theo thứ tự đã được xác nhận.
        /// </summary>
        private async Task<RankingLeaderboardResponse> ResolveTieCoreAsync(
            int roundId,
            ResolveRankingTieRequest request)
        {
            if (request.OrderedTeamIds.Count < 2)
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.TieBreakRequiresMultipleTeams);
            }

            if (request.OrderedTeamIds.Distinct().Count()
                != request.OrderedTeamIds.Count)
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.TieBreakTeamDuplicated);
            }

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(entity => entity.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            var rankingRepository =
                _unitOfWork.GetRepository<Domain.Entities.Ranking>();

            // GetAllAsync trả entity no-tracking nên phải gọi Update trước SaveChangesAsync.
            var rankings = await rankingRepository
                .GetAllAsync(ranking => ranking.RoundId == roundId);

            if (!rankings.Any())
                throw new NotFoundException(ErrorMessages.Ranking.NotFound);

            var requestedTeamIdSet = request.OrderedTeamIds.ToHashSet();
            var selectedRankings = rankings
                .Where(ranking => requestedTeamIdSet.Contains(ranking.TeamId))
                .ToList();

            if (selectedRankings.Count != request.OrderedTeamIds.Count)
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.TieBreakTeamNotInRanking);
            }

            var tiedRankPosition = selectedRankings[0].RankPosition;

            if (selectedRankings.Any(
                    ranking => ranking.RankPosition != tiedRankPosition))
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.TieBreakTeamsNotSameRank);
            }

            var completeTieTeamIdSet = rankings
                .Where(ranking => ranking.RankPosition == tiedRankPosition)
                .Select(ranking => ranking.TeamId)
                .ToHashSet();

            // Bắt buộc gửi đủ nhóm tie để tránh chỉ xử lý một phần và làm sai thứ hạng.
            if (!completeTieTeamIdSet.SetEquals(requestedTeamIdSet))
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.TieBreakGroupIncomplete);
            }

            var rankingByTeamId = selectedRankings.ToDictionary(
                ranking => ranking.TeamId);

            for (var index = 0; index < request.OrderedTeamIds.Count; index++)
            {
                var teamId = request.OrderedTeamIds[index];
                rankingByTeamId[teamId].RankPosition = tiedRankPosition + index;
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
                // Nếu vẫn còn tie tại cutoff, toàn bộ đội phải chờ trước khi Round tiếp theo được mở.
                ranking.IsAdvancing = round.AdvancingSlots.HasValue
                    && !unresolvedAdvancingRank.HasValue
                    && ranking.RankPosition <= round.AdvancingSlots.Value;

                rankingRepository.Update(ranking);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetLeaderboardByRoundAsync(roundId);
        }

        /// <summary>
        /// Lấy bảng xếp hạng đã tính của 1 round từ database, không tính lại.
        /// </summary>
        public async Task<RankingLeaderboardResponse> GetLeaderboardByRoundAsync(int roundId)
        {
            // Bước 1: Kiểm tra Round có tồn tại.
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            // Bước 2: Lấy các Ranking hợp lệ đã tính của Round này.
            // Ranking hợp lệ là ranking của team chưa bị xóa và chưa bị loại.
            var rankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(r => r.RoundId == roundId
                                  && !r.Team.IsDeleted
                                  && r.Team.Status != TeamConstants.Status.Disqualified);

            if (!rankings.Any())
            {
                return new RankingLeaderboardResponse
                {
                    RoundId = round.Id,
                    RoundName = round.Name,
                    AdvancingSlots = round.AdvancingSlots,
                    TotalTeams = 0,
                    CalculatedAt = null,
                    Rankings = new List<RankingResponse>()
                };
            }

            // Bước 3: Lấy tên Team.
            var teamIds = rankings.Select(r => r.TeamId).Distinct().ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));
            var teamNameDict = teams.ToDictionary(t => t.Id, t => t.TeamName);

            // Bước 4: Chuyển dữ liệu sang response DTO.
            var rankingResponses = rankings
                .OrderBy(r => r.RankPosition)
                .Select(r => MapToRankingResponse(
                    r, round.Name, teamNameDict.GetValueOrDefault(r.TeamId, string.Empty)))
                .ToList();

            return new RankingLeaderboardResponse
            {
                RoundId = round.Id,
                RoundName = round.Name,
                AdvancingSlots = round.AdvancingSlots,
                TotalTeams = rankingResponses.Count,
                CalculatedAt = rankings.Max(r => r.CalculatedAt),
                Rankings = rankingResponses
            };
        }

        /// <summary>
        /// Lấy ranking của 1 team cụ thể trong 1 round — trả lỗi 404 nếu chưa tính ranking
        /// </summary>
        public async Task<RankingResponse> GetTeamRankingAsync(int roundId, Guid teamId)
        {
            // Bước 1: Kiểm tra Round có tồn tại.
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            // Bước 2: Kiểm tra Team có tồn tại.
            var team = await _unitOfWork.GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId
                                            && !t.IsDeleted
                                            && t.Status != TeamConstants.Status.Disqualified);

            if (team == null)
                throw new NotFoundException(ErrorMessages.Team.NotFound);

            // Bước 3: Tìm ranking của Team trong Round này.
            var ranking = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetFirstOrDefaultAsync(r => r.RoundId == roundId && r.TeamId == teamId);

            if (ranking == null)
                throw new NotFoundException(ErrorMessages.Ranking.NotFound);

            // Bước 4: Chuyển dữ liệu sang response DTO.
            return MapToRankingResponse(ranking, round.Name, team.TeamName);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Ngăn calculate Ranking và xử lý tie-break chạy đồng thời trên cùng một Round.
        /// </summary>
        private static async Task<T> ExecuteWithRoundRankingLockAsync<T>(
            int roundId,
            Func<Task<T>> action)
        {
            var rankingLock = RoundRankingLocks.GetOrAdd(
                roundId,
                _ => new SemaphoreSlim(1, 1));

            await rankingLock.WaitAsync();

            try
            {
                return await action();
            }
            finally
            {
                rankingLock.Release();
            }
        }

        /// <summary>
        /// Tìm hạng đồng hạng đang nằm đúng tại ranh giới chọn đội đi tiếp.
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
        /// Đảm bảo chỉ tính ranking khi Round đang ở giai đoạn chấm điểm.
        /// </summary>
        private static void EnsureRoundCanCalculateRanking(Round round)
        {
            if (!string.Equals(
                    round.Status,
                    RoundConstants.Status.Scoring,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.RoundStatusInvalidForCalculation);
            }
        }

        /// <summary>
        /// Lấy danh sách judge được assign vào round và còn quyền Judge hợp lệ trong event.
        /// </summary>
        private async Task<List<Guid>> GetActiveAssignedJudgeIdsAsync(Round round)
        {
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var judgeAssigns = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetAllAsync(ja => ja.RoundId == round.Id);

            var assignedJudgeIdSet = judgeAssigns
                .Select(ja => ja.JudgeId)
                .ToHashSet();

            // JudgeAssign chỉ chứng minh được phân công;
            // EventAccount mới chứng minh judge còn quyền hợp lệ trong Event.
            var activeJudgeAccounts = await _unitOfWork
                .GetRepository<EventAccount>()
                .GetAllAsync(ea => assignedJudgeIdSet.Contains(ea.AccountId)
                                   && ea.EventId == track.EventId
                                   && ea.EventRole == RoleConstants.Judge
                                   && ea.Status == EventAccountConstants.Status.Approved
                                   && !ea.Event.IsDeleted
                                   && ea.Event.Status == EventConstants.Status.Active);

            var judgeIds = activeJudgeAccounts
                .Select(ea => ea.AccountId)
                .Distinct()
                .ToList();

            if (!judgeIds.Any())
                throw new BadRequestException(ErrorMessages.Ranking.NoAssignedJudges);

            return judgeIds;
        }

        /// <summary>
        /// Đảm bảo mọi judge hợp lệ đã chấm mọi tiêu chí cho mọi bài nộp hợp lệ.
        /// </summary>
        private static void EnsureAllRequiredScoresExist(
            List<Submission> submissions,
            List<Criterion> criteria,
            List<Guid> judgeIds,
            List<ScoreRecord> scoreRecords)
        {
            // Dùng bộ khóa chính xác để tránh trường hợp tổng số điểm đủ nhưng thiếu sai vị trí.
            var scoredKeys = scoreRecords
                .Select(sr => (sr.SubmissionId, sr.JudgeId, sr.CriterionId))
                .ToHashSet();

            foreach (var submission in submissions)
            {
                foreach (var judgeId in judgeIds)
                {
                    foreach (var criterion in criteria)
                    {
                        if (!scoredKeys.Contains((submission.Id, judgeId, criterion.Id)))
                            throw new BadRequestException(ErrorMessages.Ranking.ScoresNotCompleted);
                    }
                }
            }
        }

        // =============== Mapping helpers ===============

        /// <summary>
        /// Chuyển entity Ranking sang DTO trả về cho client.
        /// </summary>
        private static RankingResponse MapToRankingResponse(
            Domain.Entities.Ranking ranking, string roundName, string teamName)
        {
            return new RankingResponse
            {
                Id = ranking.Id,
                TeamId = ranking.TeamId,
                TeamName = teamName,
                RoundId = ranking.RoundId,
                RoundName = roundName,
                TotalScore = ranking.TotalScore,
                RankPosition = ranking.RankPosition,
                IsAdvancing = ranking.IsAdvancing,
                CalculatedAt = ranking.CalculatedAt
            };
        }
    }
}
