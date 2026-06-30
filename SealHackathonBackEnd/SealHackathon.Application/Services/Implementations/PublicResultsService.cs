using SealHackathon.Application.DTOs.PublicResults;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Exceptions;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Gom dữ liệu Ranking và Prize thành một response công khai cho trang Results.
    /// </summary>
    public class PublicResultsService : IPublicResultsService
    {
        private readonly IRankingService _rankingService;
        private readonly IPrizeService _prizeService;

        public PublicResultsService(
            IRankingService rankingService,
            IPrizeService prizeService)
        {
            _rankingService = rankingService;
            _prizeService = prizeService;
        }

        /// <summary>
        /// Lấy kết quả công khai của một Event.
        /// </summary>
        public async Task<PublicEventResultsResponse> GetEventResultsAsync(int eventId)
        {
            var eventRanking = await _rankingService.GetLeaderboardByEventAsync(eventId);
            var prizeWinners = await _prizeService.GetWinnersByEventAsync(eventId);

            var finalTrackRanking = eventRanking.TrackRankings.FirstOrDefault();

            if (finalTrackRanking is null)
                throw new BadRequestException(ErrorMessages.Ranking.FinalRoundRankingNotFound);

            var finalRoundRanking = finalTrackRanking.FinalRoundRanking;

            return new PublicEventResultsResponse
            {
                EventId = eventRanking.EventId,
                EventName = eventRanking.EventName,
                FinalTrackName = finalTrackRanking.TrackName,
                FinalRoundName = finalRoundRanking.RoundName,
                CalculatedAt = finalRoundRanking.CalculatedAt,
                Rankings = finalRoundRanking.Rankings
                    .OrderBy(ranking => ranking.RankPosition)
                    .ThenByDescending(ranking => ranking.TotalScore)
                    .Select(ranking => new PublicRankingTeamResponse
                    {
                        TeamName = ranking.TeamName,
                        University = ranking.University,
                        TotalScore = ranking.TotalScore,
                        RankPosition = ranking.RankPosition
                    })
                    .ToList(),
                PrizeWinners = prizeWinners
                    .OrderBy(winner => winner.RankPosition)
                    .Select(winner => new PublicPrizeWinnerResponse
                    {
                        PrizeName = winner.PrizeName,
                        PrizeDescription = winner.PrizeDescription,
                        RankPosition = winner.RankPosition,
                        Amount = winner.Amount,
                        TeamName = winner.TeamName,
                        University = winner.University,
                        TotalScore = winner.TotalScore
                    })
                    .ToList()
            };
        }
    }
}
