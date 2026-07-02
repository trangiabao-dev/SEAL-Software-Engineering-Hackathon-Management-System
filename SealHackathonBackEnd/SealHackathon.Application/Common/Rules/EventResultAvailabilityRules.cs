using SealHackathon.Domain.Entities;

namespace SealHackathon.Application.Common.Rules
{
    /// <summary>
    /// Chứa quy tắc xác định một Event đã đủ điều kiện hiển thị kết quả public hay chưa.
    /// </summary>
    public static class EventResultAvailabilityRules
    {
        private static readonly int[] RequiredPrizeRanks = { 1, 2, 3 };

        /// <summary>
        /// Kiểm tra Event đã có Final Round, Ranking và đủ Prize hạng 1, 2, 3 hay chưa.
        /// </summary>
        public static bool HasAvailableResults(
            int eventId,
            IReadOnlyCollection<Track> finalTracks,
            IReadOnlyCollection<Round> finalRounds,
            IReadOnlyCollection<Ranking> rankings,
            IReadOnlyCollection<Prize> prizes)
        {
            var finalTrack = finalTracks.FirstOrDefault(track => track.EventId == eventId);

            if (finalTrack is null)
                return false;

            var finalRound = finalRounds
                .Where(round => round.TrackId == finalTrack.Id)
                .OrderByDescending(round => round.OrderIndex)
                .FirstOrDefault();

            if (finalRound is null)
                return false;

            var hasRanking = rankings.Any(ranking => ranking.RoundId == finalRound.Id);
            var hasEnoughPrizes = HasEnoughPrizeConfiguration(eventId, prizes);

            return hasRanking && hasEnoughPrizes;
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Kiểm tra Event đã cấu hình đủ Prize hạng 1, 2 và 3 hay chưa.
        /// </summary>
        private static bool HasEnoughPrizeConfiguration(
            int eventId,
            IReadOnlyCollection<Prize> prizes)
        {
            var configuredPrizeRanks = prizes
                .Where(prize => prize.EventId == eventId)
                .Select(prize => prize.RankPosition)
                .Distinct()
                .ToHashSet();

            return RequiredPrizeRanks.All(configuredPrizeRanks.Contains);
        }
    }
}
