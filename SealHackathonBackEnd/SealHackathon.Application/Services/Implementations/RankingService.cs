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
        public async Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId)
        {
            var rankingLock = RoundRankingLocks.GetOrAdd(roundId, _ => new SemaphoreSlim(1, 1));

            await rankingLock.WaitAsync();
            try
            {
                return await CalculateRankingCoreAsync(roundId);
            }
            finally
            {
                rankingLock.Release();
            }
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

            // Bước 2: Lấy tiêu chí của Round để biết trọng số điểm.
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => c.RoundId == roundId);

            if (!criteria.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoCriteria);

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

            // Ranking chỉ tính điểm từ judge được assign và còn quyền Judge hợp lệ trong Event.
            var assignedJudgeIds = await GetActiveAssignedJudgeIdsAsync(round);

            // Bước 4: Lấy ScoreRecord hợp lệ, bỏ qua điểm hiệu chuẩn và điểm của judge không được assign.
            var allScoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => submissionIds.Contains(sr.SubmissionId)
                                   && assignedJudgeIds.Contains(sr.JudgeId)
                                   && !sr.IsCalibration);

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
                            var normalizedScore = avgScore / criterionConfig.MaxScore;

                            return normalizedScore * criterionConfig.Weight * 100;
                        })
                })
                .OrderByDescending(ts => ts.TotalScore)
                .ToList();

            // Bước 5b: Thêm các team có Submission nhưng chưa có điểm (TotalScore = 0)
            var teamIdsWithScores = teamScores.Select(ts => ts.TeamId).ToHashSet();
            var teamIdsWithoutScores = submissions
                .Select(s => s.TeamId)
                .Distinct()
                .Where(tid => !teamIdsWithScores.Contains(tid))
                .ToList();

            var allTeamScores = teamScores
                .Select(ts => (ts.TeamId, ts.TotalScore))
                .Concat(teamIdsWithoutScores.Select(tid => (TeamId: tid, TotalScore: 0.0)))
                .ToList();

            // Bước 6: Xếp hạng team, các team bằng điểm sẽ cùng hạng.
            // Ví dụ: điểm [9.5, 8.0, 8.0, 7.0] sẽ có hạng [1, 2, 2, 4].
            var rankedTeams = new List<(Guid TeamId, double TotalScore, int Rank)>();
            for (int i = 0; i < allTeamScores.Count; i++)
            {
                int rank;
                if (i == 0)
                {
                    rank = 1;
                }
                else if (Math.Abs(allTeamScores[i].TotalScore - allTeamScores[i - 1].TotalScore) < 0.0001)
                {
                    rank = rankedTeams[i - 1].Rank;
                }
                else
                {
                    rank = i + 1;
                }
                rankedTeams.Add((allTeamScores[i].TeamId, allTeamScores[i].TotalScore, rank));
            }

            // Bước 7: Xác định đội được vào vòng tiếp theo.
            var advancingSlots = round.AdvancingSlots;

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
                var isAdvancing = advancingSlots.HasValue && team.Rank <= advancingSlots.Value;

                var ranking = new Domain.Entities.Ranking
                {
                    Id = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    RoundId = roundId,
                    TotalScore = Math.Round(team.TotalScore, 4),
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
        /// Lấy bảng xếp hạng đã tính của 1 round — đọc từ DB, không tính lại
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
