using SealHackathon.Application.Ranking;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý logic xếp hạng — tính TotalScore (weighted average), gán RankPosition, xác định IsAdvancing
    /// </summary>
    public class RankingService : IRankingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RankingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tính toán ranking cho 1 round — xóa ranking cũ, tính lại từ ScoreRecord, lưu kết quả mới vào DB
        /// </summary>
        public async Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId)
        {
            // Bước 1: Validate Round tồn tại
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException("Round", roundId);

            // Bước 2: Lấy tất cả Criteria của round (để có Weight)
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => c.RoundId == roundId);

            if (!criteria.Any())
                throw new BadRequestException(
                    "Round này chưa có tiêu chí chấm điểm nào. Vui lòng tạo Criteria trước.");

            var criterionWeightDict = criteria.ToDictionary(c => c.Id, c => c.Weight);

            // Bước 3: Lấy tất cả Submissions của round (không bị disqualify)
            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllAsync(s => s.RoundId == roundId && !s.IsDisqualified);

            if (!submissions.Any())
                throw new BadRequestException(
                    "Round này chưa có bài nộp nào (hoặc tất cả đã bị disqualify).");

            var submissionIds = submissions.Select(s => s.Id).ToList();
            var submissionTeamDict = submissions.ToDictionary(s => s.Id, s => s.TeamId);

            // Bước 4: Lấy tất cả ScoreRecords (bỏ IsCalibration)
            var allScoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => submissionIds.Contains(sr.SubmissionId)
                                   && !sr.IsCalibration);

            // Bước 5: Tính TotalScore cho mỗi Team
            // Công thức: TotalScore = Σ (AVG(Score theo Judge) × Weight) cho mỗi Criterion
            var teamScores = allScoreRecords
                .GroupBy(sr => submissionTeamDict[sr.SubmissionId])
                .Select(teamGroup => new
                {
                    TeamId = teamGroup.Key,
                    TotalScore = teamGroup
                        .GroupBy(sr => sr.CriterionId)
                        .Sum(criterionGroup =>
                        {
                            var avgScore = criterionGroup.Average(sr => sr.Score);
                            var weight = criterionWeightDict.GetValueOrDefault(criterionGroup.Key, 0);
                            return avgScore * weight;
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

            // Bước 6: Gán RankPosition (cùng điểm → cùng hạng)
            // Ví dụ: Score = [9.5, 8.0, 8.0, 7.0] → Rank = [1, 2, 2, 4]
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

            // Bước 7: Xác định IsAdvancing
            var advancingSlots = round.AdvancingSlots;

            var now = DateTime.UtcNow;

            // Bước 8: Xóa ranking cũ → Insert ranking mới
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

            // Bước 9: Map sang Response DTO
            var teamIds = rankedTeams.Select(r => r.TeamId).ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));
            var teamNameDict = teams.ToDictionary(t => t.Id, t => t.TeamName);

            var rankingResponses = newRankings
                .OrderBy(r => r.RankPosition)
                .Select(r => new RankingResponse
                {
                    Id = r.Id,
                    TeamId = r.TeamId,
                    TeamName = teamNameDict.GetValueOrDefault(r.TeamId, string.Empty),
                    RoundId = r.RoundId,
                    RoundName = round.Name,
                    TotalScore = r.TotalScore,
                    RankPosition = r.RankPosition,
                    IsAdvancing = r.IsAdvancing,
                    CalculatedAt = r.CalculatedAt
                })
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
            // Bước 1: Validate Round tồn tại
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException("Round", roundId);

            // Bước 2: Lấy tất cả Ranking đã tính của round này
            var rankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(r => r.RoundId == roundId);

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

            // Bước 3: Lấy tên Team
            var teamIds = rankings.Select(r => r.TeamId).Distinct().ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));
            var teamNameDict = teams.ToDictionary(t => t.Id, t => t.TeamName);

            // Bước 4: Map sang Response DTO
            var rankingResponses = rankings
                .OrderBy(r => r.RankPosition)
                .Select(r => new RankingResponse
                {
                    Id = r.Id,
                    TeamId = r.TeamId,
                    TeamName = teamNameDict.GetValueOrDefault(r.TeamId, string.Empty),
                    RoundId = r.RoundId,
                    RoundName = round.Name,
                    TotalScore = r.TotalScore,
                    RankPosition = r.RankPosition,
                    IsAdvancing = r.IsAdvancing,
                    CalculatedAt = r.CalculatedAt
                })
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
            // Bước 1: Validate Round tồn tại
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException("Round", roundId);

            // Bước 2: Validate Team tồn tại
            var team = await _unitOfWork
                .GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new NotFoundException("Team", teamId);

            // Bước 3: Tìm ranking của team trong round này
            var ranking = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetFirstOrDefaultAsync(r => r.RoundId == roundId && r.TeamId == teamId);

            if (ranking == null)
                throw new NotFoundException(
                    "Ranking", $"team '{team.TeamName}' in round '{round.Name}'");

            // Bước 4: Map sang Response DTO
            return new RankingResponse
            {
                Id = ranking.Id,
                TeamId = ranking.TeamId,
                TeamName = team.TeamName,
                RoundId = ranking.RoundId,
                RoundName = round.Name,
                TotalScore = ranking.TotalScore,
                RankPosition = ranking.RankPosition,
                IsAdvancing = ranking.IsAdvancing,
                CalculatedAt = ranking.CalculatedAt
            };
        }
    }
}
