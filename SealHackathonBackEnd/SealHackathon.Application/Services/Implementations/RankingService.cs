using SealHackathon.Application.Common.Calculations;
using SealHackathon.Application.Common.Exports;
using SealHackathon.Application.Common.Rules;
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

        private static readonly string[] RankingExportHeaders =
        {
            "EventId",
            "EventName",
            "TrackId",
            "TrackName",
            "RoundId",
            "RoundName",
            "TeamId",
            "TeamName",
            "University",
            "TotalScore",
            "RankPosition",
            "IsAdvancing",
            "CalculatedAt"
        };

        private static readonly IReadOnlyDictionary<int, string> RankingExportNumberFormats =
            new Dictionary<int, string>
            {
                [10] = "0.00",
                [13] = "yyyy-mm-dd hh:mm:ss"
            };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITieBreakService _tieBreakService;

        public RankingService(
            IUnitOfWork unitOfWork,
            ITieBreakService tieBreakService)
        {
            _unitOfWork = unitOfWork;
            _tieBreakService = tieBreakService;
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

            // Bước 3: Lấy danh sách đội phải tham gia Round từ RoundTeam.
            // Ranking phải dựa trên đội được phân vào Round, không dựa trên Submission,
            // vì đội không nộp bài vẫn phải xuất hiện trong bảng xếp hạng với 0 điểm.
            var roundTeams = await _unitOfWork
                .GetRepository<RoundTeam>()
                .GetAllAsync(roundTeam =>
                    roundTeam.RoundId == roundId
                    && !roundTeam.Team.IsDeleted
                    && roundTeam.Team.Status != TeamConstants.Status.Disqualified);

            var roundTeamIds = roundTeams
                .Select(roundTeam => roundTeam.TeamId)
                .Distinct()
                .ToList();

            if (!roundTeamIds.Any())
                throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoAssignedTeams);

            // Bước 4: Lấy các bài nộp hợp lệ của những đội được phân vào Round.
            // Đội không có Submission sẽ không bị lỗi ở đây; hệ thống sẽ cho 0 điểm ở bước tính Ranking.
            var submissions = await _unitOfWork
                .GetRepository<Submission>()
                .GetAllAsync(submission =>
                    submission.RoundId == roundId
                    && roundTeamIds.Contains(submission.TeamId)
                    && !submission.IsDisqualified
                    && !submission.Team.IsDeleted
                    && submission.Team.Status != TeamConstants.Status.Disqualified);

            var submissionIds = submissions.Select(s => s.Id).ToList();
            var submissionTeamDict = submissions.ToDictionary(s => s.Id, s => s.TeamId);
            var teamIdsWithValidSubmission = submissions
                .Select(submission => submission.TeamId)
                .Distinct()
                .ToHashSet();

            // Ranking chỉ tính điểm từ judge được assign và còn quyền Judge hợp lệ trong Event.
            var assignedJudgeIds = await GetActiveAssignedJudgeIdsAsync(round);

            // Bước 5: Lấy ScoreRecord hợp lệ, chỉ tính điểm của Judge đang được assign.
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

            // Bước 6: Tính TotalScore cho từng team có Submission.
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

            // Bước 6b: Thêm các team không có Submission hợp lệ với TotalScore = 0.
            // Không tạo ScoreRecord giả vì ScoreRecord chỉ lưu điểm Judge thật sự đã nhập.
            var teamIdsWithoutValidSubmission = roundTeamIds
                .Where(teamId => !teamIdsWithValidSubmission.Contains(teamId))
                .ToList();

            // Làm tròn trước khi so sánh để điểm xếp hạng khớp điểm lưu trong database.
            var allTeamScores = teamScores
                .Select(teamScore => (
                    TeamId: teamScore.TeamId,
                    TotalScore: Math.Round(teamScore.TotalScore, 4)))
                .Concat(teamIdsWithoutValidSubmission.Select(teamId => (
                    TeamId: teamId,
                    TotalScore: 0.0)))
                .OrderByDescending(team => team.TotalScore)
                .ToList();

            // Bước 6: Xếp điểm giảm dần; nếu bằng TotalScore thì giữ đồng hạng để xử lý tie-break.
            var rankedTeams = new List<(Guid TeamId, double TotalScore, int Rank)>();
            for (var index = 0; index < allTeamScores.Count; index++)
            {
                var rank = index + 1;

                if (index > 0)
                {
                    var currentTeam = allTeamScores[index];
                    var previousTeam = allTeamScores[index - 1];

                    // Chỉ cần bằng điểm là đồng hạng; không dùng thời gian nộp để tự phá tie nữa.
                    if (currentTeam.TotalScore == previousTeam.TotalScore)
                    {
                        rank = rankedTeams[index - 1].Rank;
                    }
                }

                rankedTeams.Add((
                    allTeamScores[index].TeamId,
                    allTeamScores[index].TotalScore,
                    rank));
            }

            // Xử lý áp dụng lại hạng cho các nhóm đã có tie-break Completed
            var completedTieBreakOrders = await _tieBreakService.GetCompletedTieBreakOrdersAsync(roundId);

            if (completedTieBreakOrders.Any())
            {
                var newRankedTeams = new List<(Guid TeamId, double TotalScore, int Rank)>();

                // rankedTeams đang xếp theo TotalScore giảm dần, các team đồng hạng sẽ có chung Rank.
                var groupedByRank = rankedTeams.GroupBy(t => t.Rank).OrderBy(g => g.Key).ToList();

                foreach (var group in groupedByRank)
                {
                    if (completedTieBreakOrders.TryGetValue(group.Key, out var orderedTeamIds))
                    {
                        var groupTeamIds = group.Select(t => t.TeamId).ToHashSet();
                        
                        // Nếu nhóm đồng hạng gốc vẫn khớp với nhóm đội trong phiên tie-break đã xử lý
                        if (groupTeamIds.SetEquals(orderedTeamIds))
                        {
                            var teamByOrderedIndex = orderedTeamIds.Select((teamId, index) =>
                            {
                                var team = group.First(t => t.TeamId == teamId);
                                return (team.TeamId, team.TotalScore, Rank: group.Key + index);
                            }).ToList();

                            newRankedTeams.AddRange(teamByOrderedIndex);
                        }
                        else
                        {
                            // Điểm cơ bản đã thay đổi làm vỡ nhóm đồng hạng gốc, bỏ qua tie-break này.
                            newRankedTeams.AddRange(group);
                        }
                    }
                    else
                    {
                        newRankedTeams.AddRange(group);
                    }
                }

                // Cập nhật lại danh sách sau khi đã tách hạng.
                rankedTeams = newRankedTeams.OrderBy(t => t.Rank).ToList();
            }

            // Bước 7: Xác định đội được vào vòng tiếp theo.
            // AdvancingSlots null đánh dấu vòng chung kết nên không có đội đi tiếp.
            var advancingSlots = round.AdvancingSlots;
            var unresolvedAdvancingRank = GetUnresolvedAdvancingRank(
                rankedTeams,
                advancingSlots);
            var importantTieRanks = GetImportantTieRanks(
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

            await DisqualifyTeamsWithoutValidSubmissionAsync(
                teamIdsWithoutValidSubmission,
                round,
                now);

            // Bước 9: Sau khi Ranking đã lưu, tự tạo phiên tie-break cho các hạng quan trọng còn đồng điểm.
            var tieBreakSessionByRank = new Dictionary<int, Guid>();
            foreach (var tieRank in importantTieRanks)
            {
                var session = await _tieBreakService.CreateSessionIfNotExistsAsync(roundId, tieRank);
                if (session != null)
                {
                    tieBreakSessionByRank[tieRank] = session.Id;
                }
            }

            // Bước 10: Chuyển dữ liệu sang response DTO.
            var teamIds = rankedTeams.Select(r => r.TeamId).ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));
            var teamById = teams.ToDictionary(team => team.Id);

            var rankingResponses = newRankings
                .OrderBy(ranking => ranking.RankPosition)
                .Select(ranking =>
                {
                    // Ranking public cần đủ tên đội và trường đại học, nên map từ Team entity thay vì chỉ lấy TeamName.
                    teamById.TryGetValue(ranking.TeamId, out var team);
                    var hasTieBreak = tieBreakSessionByRank.TryGetValue(ranking.RankPosition, out var sessionId);

                    return MapToRankingResponse(
                        ranking,
                        round.Name,
                        team?.TeamName ?? string.Empty,
                        team?.University ?? string.Empty,
                        hasTieBreak ? sessionId : null);
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
        /// Lấy bảng xếp hạng đã tính của 1 round từ database, không tính lại.
        /// </summary>
        public async Task<RankingLeaderboardResponse> GetLeaderboardByRoundAsync(int roundId)
        {
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            var rankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(r => r.RoundId == roundId
                                  && !r.Team.IsDeleted
                                  && r.Team.Status != TeamConstants.Status.Disqualified);

            var teamById = new Dictionary<Guid, Team>();

            if (rankings.Count > 0)
            {
                var teamIds = rankings
                    .Select(ranking => ranking.TeamId)
                    .Distinct()
                    .ToList();

                var teams = await _unitOfWork
                    .GetRepository<Team>()
                    .GetAllAsync(team => teamIds.Contains(team.Id));

                teamById = teams.ToDictionary(team => team.Id);
            }

            var tieBreakSessions = await _tieBreakService.GetSessionsByRoundAsync(roundId);
            var tieBreakSessionByRank = tieBreakSessions
                .Where(s => s.Status == TieBreakConstants.Status.PendingScoring)
                .ToDictionary(s => s.RankPosition, s => s.Id);

            return BuildLeaderboardResponse(round, rankings, teamById, tieBreakSessionByRank);
        }


        /// <summary>
        /// Lấy Ranking chung cuộc của Event từ Final Round thuộc Track Final.
        /// </summary>
        public async Task<EventRankingResponse> GetLeaderboardByEventAsync(int eventId)
        {
            var eventEntity = await _unitOfWork
                .GetRepository<Event>()
                .GetFirstOrDefaultAsync(entity => entity.Id == eventId && !entity.IsDeleted);

            if (eventEntity is null)
                throw new NotFoundException(ErrorMessages.Ranking.EventNotFound);

            EnsureEventCompleted(eventEntity);

            var tracks = await _unitOfWork
                .GetRepository<Track>()
                .GetAllAsync(track => track.EventId == eventEntity.Id && !track.IsDeleted);

            var finalTrack = EventFinalRoundRules.GetFinalTrack(tracks);
            var rounds = await _unitOfWork
                .GetRepository<Round>()
                .GetAllAsync(round => round.TrackId == finalTrack.Id);

            var finalRound = EventFinalRoundRules.GetFinalRound(finalTrack, rounds);
            EnsureFinalRoundClosed(finalRound);

            // Event Ranking chung cuộc chỉ lấy từ Final Round của Track Final,
            // không gom điểm từ các Track vòng loại để tránh công bố sai top 3 toàn Event.
            var rankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(ranking => ranking.RoundId == finalRound.Id
                                        && !ranking.Team.IsDeleted
                                        && ranking.Team.Status != TeamConstants.Status.Disqualified);

            if (!rankings.Any())
                throw new BadRequestException(ErrorMessages.Ranking.FinalRoundRankingNotFound);

            var teamIds = rankings.Select(ranking => ranking.TeamId).Distinct().ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(team => teamIds.Contains(team.Id));

            var teamById = teams.ToDictionary(team => team.Id);

            var tieBreakSessions = await _tieBreakService.GetSessionsByRoundAsync(finalRound.Id);
            var tieBreakSessionByRank = tieBreakSessions
                .Where(s => s.Status == TieBreakConstants.Status.PendingScoring)
                .ToDictionary(s => s.RankPosition, s => s.Id);

            var finalRoundRanking = BuildLeaderboardResponse(
                finalRound,
                rankings,
                teamById,
                tieBreakSessionByRank);
            EnsureLeaderboardCalculated(finalRoundRanking);

            var trackRankings = new List<TrackFinalRankingResponse>
            {
                new()
                {
                    TrackId = finalTrack.Id,
                    TrackName = finalTrack.Name,
                    FinalRoundRanking = finalRoundRanking
                }
            };

            var eventTop3 = finalRoundRanking.Rankings
                .OrderBy(ranking => ranking.RankPosition)
                .ThenByDescending(ranking => ranking.TotalScore)
                .Take(3)
                .ToList();

            return new EventRankingResponse
            {
                EventId = eventEntity.Id,
                EventName = eventEntity.Name,
                TotalTracks = trackRankings.Count,
                TrackRankings = trackRankings,
                EventTop3 = eventTop3
            };
        }

        /// <summary>
        /// Xuất bảng xếp hạng của một Round đã đóng ra file XLSX.
        /// </summary>
        public async Task<byte[]> ExportLeaderboardByRoundAsync(int roundId)
        {
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(entity => entity.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            EnsureRoundClosedForExport(round);

            var leaderboard = await GetLeaderboardByRoundAsync(round.Id);
            EnsureRankingReportAvailable(leaderboard);

            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(entity => entity.Id == round.TrackId && !entity.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var eventEntity = await _unitOfWork
                .GetRepository<Event>()
                .GetFirstOrDefaultAsync(entity => entity.Id == track.EventId && !entity.IsDeleted);

            if (eventEntity is null)
                throw new NotFoundException(ErrorMessages.Ranking.EventNotFound);

            var teamById = await GetTeamsForExportAsync(leaderboard.Rankings);
            var rows = MapRankingExportRows(
                eventEntity.Id,
                eventEntity.Name,
                track.Id,
                track.Name,
                leaderboard,
                teamById);

            return CreateRankingWorkbook("Round Ranking", rows);
        }

        /// <summary>
        /// Xuất bảng xếp hạng chung cuộc của Event ra file XLSX.
        /// </summary>
        public async Task<byte[]> ExportLeaderboardByEventAsync(int eventId)
        {
            var eventRanking = await GetLeaderboardByEventAsync(eventId);
            var rankingResponses = eventRanking.TrackRankings
                .SelectMany(trackRanking => trackRanking.FinalRoundRanking.Rankings)
                .ToList();

            var teamById = await GetTeamsForExportAsync(rankingResponses);
            var rows = eventRanking.TrackRankings
                .OrderBy(trackRanking => trackRanking.TrackName)
                .SelectMany(trackRanking => MapRankingExportRows(
                    eventRanking.EventId,
                    eventRanking.EventName,
                    trackRanking.TrackId,
                    trackRanking.TrackName,
                    trackRanking.FinalRoundRanking,
                    teamById))
                .ToList();

            return CreateRankingWorkbook("Event Rankings", rows);
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
            return MapToRankingResponse(
                ranking,
                round.Name,
                team.TeamName,
                team.University);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Kiểm tra Round đã đóng trước khi xuất báo cáo Ranking chính thức.
        /// </summary>
        private static void EnsureRoundClosedForExport(Round round)
        {
            if (!string.Equals(
                    round.Status,
                    RoundConstants.Status.Closed,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.Ranking.RoundNotClosedForExport);
            }
        }

        /// <summary>
        /// Kiểm tra bảng xếp hạng có dữ liệu để xuất báo cáo.
        /// </summary>
        private static void EnsureRankingReportAvailable(RankingLeaderboardResponse leaderboard)
        {
            if (leaderboard.Rankings.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.RankingReportNotFound);
        }

        /// <summary>
        /// Lấy thông tin Team của toàn bộ dòng Ranking bằng một truy vấn.
        /// </summary>
        private async Task<Dictionary<Guid, Team>> GetTeamsForExportAsync(
            IEnumerable<RankingResponse> rankings)
        {
            var teamIds = rankings
                .Select(ranking => ranking.TeamId)
                .Distinct()
                .ToList();

            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(team => teamIds.Contains(team.Id));

            return teams.ToDictionary(team => team.Id, team => team);
        }

        /// <summary>
        /// Chuyển bảng xếp hạng của một Round thành các dòng báo cáo XLSX.
        /// </summary>
        private static List<RankingExportRow> MapRankingExportRows(
            int eventId,
            string eventName,
            int trackId,
            string trackName,
            RankingLeaderboardResponse leaderboard,
            IReadOnlyDictionary<Guid, Team> teamById)
        {
            var rows = new List<RankingExportRow>();

            foreach (var ranking in leaderboard.Rankings.OrderBy(item => item.RankPosition))
            {
                if (!teamById.TryGetValue(ranking.TeamId, out var team))
                    throw new NotFoundException(ErrorMessages.Team.NotFound);

                rows.Add(new RankingExportRow
                {
                    EventId = eventId,
                    EventName = eventName,
                    TrackId = trackId,
                    TrackName = trackName,
                    RoundId = leaderboard.RoundId,
                    RoundName = leaderboard.RoundName,
                    TeamId = ranking.TeamId,
                    TeamName = ranking.TeamName,
                    University = team.University,
                    TotalScore = ranking.TotalScore,
                    RankPosition = ranking.RankPosition,
                    IsAdvancing = ranking.IsAdvancing,
                    CalculatedAt = ranking.CalculatedAt
                });
            }

            return rows;
        }

        /// <summary>
        /// Tạo file Ranking XLSX từ các dòng báo cáo đã chuẩn hóa.
        /// </summary>
        private static byte[] CreateRankingWorkbook(
            string sheetName,
            IReadOnlyCollection<RankingExportRow> rankingRows)
        {
            var rows = rankingRows
                .Select(row => new object?[]
                {
                    row.EventId,
                    row.EventName,
                    row.TrackId,
                    row.TrackName,
                    row.RoundId,
                    row.RoundName,
                    row.TeamId,
                    row.TeamName,
                    row.University,
                    row.TotalScore,
                    row.RankPosition,
                    row.IsAdvancing,
                    row.CalculatedAt
                })
                .ToList();

            return ExcelExportHelper.CreateWorkbook(
                sheetName,
                RankingExportHeaders,
                rows,
                RankingExportNumberFormats);
        }

        /// <summary>
        /// Kiểm tra vòng chung kết đã được đóng trước khi công bố Ranking.
        /// </summary>
        private static void EnsureFinalRoundClosed(Round finalRound)
        {
            if (!string.Equals(
                    finalRound.Status,
                    RoundConstants.Status.Closed,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.Ranking.FinalRoundNotClosed);
            }
        }

        /// <summary>
        /// Kiểm tra Final Round đã có dữ liệu Ranking.
        /// </summary>
        private static void EnsureLeaderboardCalculated(RankingLeaderboardResponse leaderboard)
        {
            if (leaderboard.Rankings.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.FinalRoundRankingNotFound);
        }

        /// <summary>
        /// Chỉ cho tổng hợp Ranking chính thức khi Event đã hoàn thành.
        /// </summary>
        private static void EnsureEventCompleted(Event eventEntity)
        {
            if (!string.Equals(
                    eventEntity.Status,
                    EventConstants.Status.Completed,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.Ranking.EventNotCompleted);
            }
        }

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
        /// Chuyển các đội không có bài nộp hợp lệ sang trạng thái bị loại sau khi Ranking đã lưu điểm 0.
        /// </summary>
        private async Task DisqualifyTeamsWithoutValidSubmissionAsync(
            IReadOnlyCollection<Guid> teamIdsWithoutValidSubmission,
            Round round,
            DateTime now)
        {
            if (!teamIdsWithoutValidSubmission.Any())
                return;

            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(team =>
                    teamIdsWithoutValidSubmission.Contains(team.Id)
                    && !team.IsDeleted
                    && team.Status != TeamConstants.Status.Disqualified);

            foreach (var team in teams)
            {
                // Ranking phải lưu 0 điểm trước, sau đó mới đánh dấu loại để không mất dữ liệu xếp hạng.
                team.Status = TeamConstants.Status.Disqualified;
                team.DisqualifyReason = $"Không có bài nộp hợp lệ ở vòng thi {round.Name}.";
                team.UpdatedAt = now;

                _unitOfWork.GetRepository<Team>().Update(team);
            }

            await _unitOfWork.SaveChangesAsync();
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
        /// Tìm các hạng đồng điểm cần tạo tie-break vì ảnh hưởng đội đi tiếp hoặc Top 3 chung cuộc.
        /// </summary>
        private static List<int> GetImportantTieRanks(
            IReadOnlyList<(Guid TeamId, double TotalScore, int Rank)> rankedTeams,
            int? advancingSlots)
        {
            var importantTieRanks = new List<int>();
            var unresolvedAdvancingRank = GetUnresolvedAdvancingRank(
                rankedTeams,
                advancingSlots);

            if (unresolvedAdvancingRank.HasValue)
                importantTieRanks.Add(unresolvedAdvancingRank.Value);

            if (!advancingSlots.HasValue)
            {
                // Final Round không có đội đi tiếp, nên chỉ cần khóa tie trong Top 3 để công bố Event/Prize.
                importantTieRanks.AddRange(rankedTeams
                    .Where(team => team.Rank <= 3)
                    .GroupBy(team => team.Rank)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key));
            }

            return importantTieRanks
                .Distinct()
                .OrderBy(rank => rank)
                .ToList();
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
        /// Chuyển Ranking entities thành response bảng xếp hạng dùng chung.
        /// </summary>
        private static RankingLeaderboardResponse BuildLeaderboardResponse(
            Round round,
            IReadOnlyCollection<Domain.Entities.Ranking> rankings,
            IReadOnlyDictionary<Guid, Team> teamById,
            IReadOnlyDictionary<int, Guid>? tieBreakSessionByRank = null)
        {
            if (rankings.Count == 0)
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

            var responses = rankings
                .OrderBy(ranking => ranking.RankPosition)
                .Select(ranking =>
                {
                    // Dùng Team entity để response Ranking có đủ TeamName và University cho mọi đội.
                    teamById.TryGetValue(ranking.TeamId, out var team);
                    Guid sessionId = Guid.Empty;
                    var hasTieBreak = tieBreakSessionByRank?.TryGetValue(ranking.RankPosition, out sessionId) == true;

                    return MapToRankingResponse(
                        ranking,
                        round.Name,
                        team?.TeamName ?? string.Empty,
                        team?.University ?? string.Empty,
                        hasTieBreak ? sessionId : null);
                })
                .ToList();

            return new RankingLeaderboardResponse
            {
                RoundId = round.Id,
                RoundName = round.Name,
                AdvancingSlots = round.AdvancingSlots,
                TotalTeams = responses.Count,
                CalculatedAt = rankings.Max(ranking => ranking.CalculatedAt),
                Rankings = responses
            };
        }

        /// <summary>
        /// Chuyển entity Ranking sang DTO trả về cho client.
        /// </summary>
        private static RankingResponse MapToRankingResponse(
            Domain.Entities.Ranking ranking,
            string roundName,
            string teamName,
            string university,
            Guid? tieBreakSessionId = null)
        {
            return new RankingResponse
            {
                Id = ranking.Id,
                TeamId = ranking.TeamId,
                TeamName = teamName,
                University = university,
                RoundId = ranking.RoundId,
                RoundName = roundName,
                TotalScore = ranking.TotalScore,
                RankPosition = ranking.RankPosition,
                IsAdvancing = ranking.IsAdvancing,
                CalculatedAt = ranking.CalculatedAt,
                TieBreakSessionId = tieBreakSessionId
            };
        }
    }
}
