using SealHackathon.Application.Ranking;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Handles ranking logic — calculates TotalScore (weighted average), assigns RankPosition, and determines IsAdvancing.
    /// </summary>
    public class RankingService : IRankingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RankingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Calculates (or recalculates) the ranking for a round — deletes existing rankings,
        /// recomputes from ScoreRecords, and saves the result to the database.
        /// </summary>
        public async Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId)
        {
            // Step 1: Verify Round exists
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException("Round", roundId);

            // Step 2: Fetch criteria for this round (needed for Weight values)
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => c.RoundId == roundId);

            if (!criteria.Any())
                throw new BadRequestException(
                    "This round has no scoring criteria. Please create criteria before calculating rankings.");

            var criterionWeightDict = criteria.ToDictionary(c => c.Id, c => c.Weight);

            // Step 3: Build a set of disqualified/deleted team IDs to exclude from ranking
            var disqualifiedTeamIds = (await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => t.Status == TeamConstants.Status.Disqualified || t.IsDeleted))
                .Select(t => t.Id)
                .ToHashSet();

            // Step 4: Fetch all valid submissions (not disqualified, team not disqualified/deleted)
            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllAsync(s => s.RoundId == roundId
                                  && !s.IsDisqualified
                                  && !disqualifiedTeamIds.Contains(s.TeamId));

            if (!submissions.Any())
                throw new BadRequestException(
                    "This round has no valid submissions (all may have been disqualified).");

            var submissionIds = submissions.Select(s => s.Id).ToList();
            var submissionTeamDict = submissions.ToDictionary(s => s.Id, s => s.TeamId);

            // Step 5: Fetch ScoreRecords (exclude calibration records)
            var allScoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => submissionIds.Contains(sr.SubmissionId)
                                   && !sr.IsCalibration);

            // Step 6: Calculate TotalScore per team
            // Formula: TotalScore = sum of (AVG score per criterion × criterion weight)
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

            // Step 7: Assign RankPosition (tied scores share the same rank)
            // Example: scores [9.5, 8.0, 8.0, 7.0] → ranks [1, 2, 2, 4]
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

            // Step 3: Filter out rankings for teams that were disqualified or deleted after ranking was calculated
            if (rankings.Any())
            {
                var rankingTeamIds = rankings.Select(r => r.TeamId).Distinct().ToList();
                var disqualifiedTeams = await _unitOfWork
                    .GetRepository<Team>()
                    .GetAllAsync(t => rankingTeamIds.Contains(t.Id)
                                      && (t.Status == TeamConstants.Status.Disqualified || t.IsDeleted));
                var disqualifiedTeamIds = disqualifiedTeams.Select(t => t.Id).ToHashSet();
                rankings = rankings.Where(r => !disqualifiedTeamIds.Contains(r.TeamId)).ToList();
            }

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
            // Step 1: Verify Round exists
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round == null)
                throw new NotFoundException("Round", roundId);

            // Step 2: Verify Team exists
            var team = await _unitOfWork
                .GetRepository<Team>()
                .GetFirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
                throw new NotFoundException("Team", teamId);

            // Step 3: Find the ranking entry for this team in this round
            var ranking = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetFirstOrDefaultAsync(r => r.RoundId == roundId && r.TeamId == teamId);

            if (ranking == null)
                throw new NotFoundException(
                    "Ranking", $"team '{team.TeamName}' in round '{round.Name}'");

            // Step 4: Map to response DTO
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
