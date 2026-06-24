using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;

namespace SealHackathon.Application.Common.Rules
{
    /// <summary>
    /// Chứa quy tắc xác định Round chung kết của một Track.
    /// </summary>
    public static class FinalRoundRules
    {
        /// <summary>
        /// Trả về Round chung kết khi cấu hình Round của Track hợp lệ.
        /// </summary>
        public static Round GetFinalRound(
            int trackId,
            IReadOnlyCollection<Round> rounds)
        {
            var trackRounds = rounds
                .Where(round => round.TrackId == trackId)
                .ToList();

            if (trackRounds.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.TrackHasNoRounds);

            var finalRounds = trackRounds
                .Where(round => !round.AdvancingSlots.HasValue)
                .ToList();

            var hasDuplicatedOrder = trackRounds
                .GroupBy(round => round.OrderIndex)
                .Any(group => group.Count() > 1);

            var highestOrder = trackRounds.Max(round => round.OrderIndex);

            // Track chỉ hợp lệ khi có đúng một Final Round và nó nằm cuối cùng.
            if (finalRounds.Count != 1
                || finalRounds[0].OrderIndex != highestOrder
                || hasDuplicatedOrder)
            {
                throw new BadRequestException(
                    ErrorMessages.Ranking.FinalRoundConfigurationInvalid);
            }

            return finalRounds[0];
        }
    }
}
