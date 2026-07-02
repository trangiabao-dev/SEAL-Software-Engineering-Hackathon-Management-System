using SealHackathon.Application.Common.Rules;
using SealHackathon.Application.DTOs.PublicResults;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Gom dữ liệu Ranking và Prize thành response công khai cho trang Results.
    /// </summary>
    public class PublicResultsService : IPublicResultsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRankingService _rankingService;
        private readonly IPrizeService _prizeService;

        /// <summary>
        /// Khởi tạo service public results.
        /// </summary>
        public PublicResultsService(
            IUnitOfWork uow,
            IRankingService rankingService,
            IPrizeService prizeService)
        {
            _uow = uow;
            _rankingService = rankingService;
            _prizeService = prizeService;
        }

        /// <summary>
        /// Lấy danh sách Event public đang Active hoặc đã Completed.
        /// </summary>
        public async Task<List<PublicEventSummaryResponse>> GetPublicEventsAsync()
        {
            var events = await _uow.GetRepository<Event>()
                .GetAllAsync(evt =>
                    !evt.IsDeleted
                    && (evt.Status == EventConstants.Status.Active
                        || evt.Status == EventConstants.Status.Completed));

            var eventIds = events.Select(evt => evt.Id).ToList();

            var tracks = await _uow.GetRepository<Track>()
                .GetAllAsync(track =>
                    eventIds.Contains(track.EventId)
                    && track.IsFinal
                    && !track.IsDeleted);

            var finalTrackIds = tracks.Select(track => track.Id).ToList();

            var finalRounds = await _uow.GetRepository<Round>()
                .GetAllAsync(round =>
                    finalTrackIds.Contains(round.TrackId)
                    && round.AdvancingSlots == null
                    && round.Status == RoundConstants.Status.Closed);

            var finalRoundIds = finalRounds.Select(round => round.Id).ToList();

            var rankings = await _uow.GetRepository<Ranking>()
                .GetAllAsync(ranking => finalRoundIds.Contains(ranking.RoundId));

            var prizes = await _uow.GetRepository<Prize>()
                .GetAllAsync(prize => eventIds.Contains(prize.EventId));

            return events
                .OrderByDescending(evt => evt.StartDate)
                .Select(evt =>
                {
                    // Dùng rule chung để PublicResultsService và EventService hiểu ResultsAvailable giống nhau.
                    var resultsAvailable = EventResultAvailabilityRules.HasAvailableResults(
                        evt.Id,
                        tracks,
                        finalRounds,
                        rankings,
                        prizes);

                    return new PublicEventSummaryResponse
                    {
                        EventId = evt.Id,
                        EventName = evt.Name,
                        Description = evt.Description,
                        StartDate = evt.StartDate,
                        EndDate = evt.EndDate,
                        Status = evt.Status,
                        ResultsAvailable = resultsAvailable
                    };
                })
                .ToList();
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
