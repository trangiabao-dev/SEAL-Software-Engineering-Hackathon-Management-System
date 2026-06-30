using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;

namespace SealHackathon.Application.Common.Rules
{
    /// <summary>
    /// Chứa quy tắc tìm Track Final và Final Round chính thức của một Event.
    /// </summary>
    public static class EventFinalRoundRules
    {
        /// <summary>
        /// Trả về Track Final duy nhất của Event.
        /// </summary>
        public static Track GetFinalTrack(IReadOnlyCollection<Track> tracks)
        {
            if (tracks.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.EventHasNoTracks);

            var finalTracks = tracks
                .Where(track => track.IsFinal)
                .ToList();

            if (finalTracks.Count == 0)
                throw new BadRequestException(ErrorMessages.Ranking.EventFinalTrackNotFound);

            if (finalTracks.Count > 1)
                throw new BadRequestException(ErrorMessages.Ranking.EventFinalTrackDuplicated);

            return finalTracks[0];
        }

        /// <summary>
        /// Trả về Final Round thuộc Track Final.
        /// </summary>
        public static Round GetFinalRound(
            Track finalTrack,
            IReadOnlyCollection<Round> rounds)
        {
            // Dùng lại rule FinalRound hiện có để tránh hai cách hiểu khác nhau về Final Round.
            return FinalRoundRules.GetFinalRound(finalTrack.Id, rounds);
        }
    }
}
