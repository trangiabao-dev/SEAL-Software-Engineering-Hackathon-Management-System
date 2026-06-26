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

            if (unresolvedAdvancingRank.HasValue || (!advancingSlots.HasValue && rankedTeams.Where(r => r.Rank <= 3).GroupBy(r => r.Rank).Any(g => g.Count() > 1)))
            {
                var judgeAssigns = await _unitOfWork.GetRepository<JudgeAssign>().GetAllAsync(ja => ja.RoundId == roundId);
                var judgeIds = judgeAssigns.Select(ja => ja.JudgeId).Distinct().ToList();
                
                foreach (var judgeId in judgeIds)
                {
                    await _unitOfWork.GetRepository<Notification>().AddAsync(new Notification
                    {
                        AccountId = judgeId,
                        Title = "Xử lý đồng hạng",
                        Message = $"Có đội đồng hạng tại ranh giới đi tiếp hoặc Top 3 ở vòng thi '{round.Name}'. Vui lòng xem xét giải quyết Tie-break.",
                        Type = "TIE_BREAK_REQUIRED",
                        IsRead = false,
                        CreatedAt = now
                    });
                }
            }

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

            var teamNameById = new Dictionary<Guid, string>();

            if (rankings.Count > 0)
            {
                var teamIds = rankings
                    .Select(ranking => ranking.TeamId)
                    .Distinct()
                    .ToList();

                var teams = await _unitOfWork
                    .GetRepository<Team>()
                    .GetAllAsync(team => teamIds.Contains(team.Id));

                teamNameById = teams.ToDictionary(
                    team => team.Id,
                    team => team.TeamName);
            }

            return BuildLeaderboardResponse(round, rankings, teamNameById);
        }

        /// <summary>
        /// Lấy Ranking chính thức của vòng chung kết thuộc một Track.
        /// </summary>
        public async Task<TrackFinalRankingResponse> GetLeaderboardByTrackAsync(int trackId)
        {
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(entity => entity.Id == trackId && !entity.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var rounds = await _unitOfWork
                .GetRepository<Round>()
                .GetAllAsync(round => round.TrackId == track.Id);

            var finalRound = FinalRoundRules.GetFinalRound(track.Id, rounds);
            EnsureFinalRoundClosed(finalRound);

            var leaderboard = await GetLeaderboardByRoundAsync(finalRound.Id);
            EnsureLeaderboardCalculated(leaderboard);

            return new TrackFinalRankingResponse
            {
                TrackId = track.Id,
                TrackName = track.Name,
                FinalRoundRanking = leaderboard
            };
        }

        /// <summary>
        /// Tổng hợp Ranking chung kết của tất cả Track trong Event đã hoàn thành.
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

            if (tracks.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.EventHasNoTracks);

            var trackIds = tracks.Select(track => track.Id).ToList();
            var rounds = await _unitOfWork
                .GetRepository<Round>()
                .GetAllAsync(round => trackIds.Contains(round.TrackId));

            var finalRoundByTrackId = new Dictionary<int, Round>();
            foreach (var track in tracks)
            {
                var finalRound = FinalRoundRules.GetFinalRound(track.Id, rounds);
                EnsureFinalRoundClosed(finalRound);
                finalRoundByTrackId.Add(track.Id, finalRound);
            }

            var finalRoundIds = finalRoundByTrackId.Values
                .Select(round => round.Id)
                .ToList();

            // Lấy Ranking của mọi Final Round trong một query để tránh N+1.
            var rankings = await _unitOfWork
                .GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(ranking => finalRoundIds.Contains(ranking.RoundId)
                                        && !ranking.Team.IsDeleted
                                        && ranking.Team.Status != TeamConstants.Status.Disqualified);

            var rankingsByRoundId = rankings
                .GroupBy(ranking => ranking.RoundId)
                .ToDictionary(group => group.Key, group => group.ToList());

            if (finalRoundIds.Any(finalRoundId => !rankingsByRoundId.ContainsKey(finalRoundId)))
                throw new BadRequestException(ErrorMessages.Ranking.FinalRoundRankingNotFound);

            var teamIds = rankings.Select(ranking => ranking.TeamId).Distinct().ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(team => teamIds.Contains(team.Id));

            var teamNameById = teams.ToDictionary(team => team.Id, team => team.TeamName);
            var trackRankings = tracks
                .OrderBy(track => track.Name)
                .Select(track =>
                {
                    var finalRound = finalRoundByTrackId[track.Id];

                    return new TrackFinalRankingResponse
                    {
                        TrackId = track.Id,
                        TrackName = track.Name,
                        FinalRoundRanking = BuildLeaderboardResponse(
                            finalRound,
                            rankingsByRoundId[finalRound.Id],
                            teamNameById)
                    };
                })
                .ToList();

            var eventTop3 = trackRankings
                .SelectMany(tr => tr.FinalRoundRanking.Rankings)
                .OrderByDescending(r => r.TotalScore)
                .Take(3)
                .ToList();

            // Sửa lại thứ hạng (RankPosition) của Top 3 Event
            for (int i = 0; i < eventTop3.Count; i++)
            {
                if (i > 0 && eventTop3[i].TotalScore == eventTop3[i - 1].TotalScore)
                {
                    eventTop3[i].RankPosition = eventTop3[i - 1].RankPosition;
                }
                else
                {
                    eventTop3[i].RankPosition = i + 1;
                }
            }

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
        /// Xuất bảng xếp hạng chung kết của mọi Track trong Event ra file XLSX.
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
            return MapToRankingResponse(ranking, round.Name, team.TeamName);
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
        /// Chuyển Ranking entities thành response bảng xếp hạng dùng chung.
        /// </summary>
        private static RankingLeaderboardResponse BuildLeaderboardResponse(
            Round round,
            IReadOnlyCollection<Domain.Entities.Ranking> rankings,
            IReadOnlyDictionary<Guid, string> teamNameById)
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
                .Select(ranking => MapToRankingResponse(
                    ranking,
                    round.Name,
                    teamNameById.TryGetValue(ranking.TeamId, out var teamName)
                        ? teamName
                        : string.Empty))
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
