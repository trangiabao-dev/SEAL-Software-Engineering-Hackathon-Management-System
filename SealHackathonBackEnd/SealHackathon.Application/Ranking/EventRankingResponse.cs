namespace SealHackathon.Application.DTOs.Ranking
{
    /// <summary>
    /// Bảng xếp hạng chung cuộc của Event, lấy từ Final Round thuộc Track Final.
    /// </summary>
    public class EventRankingResponse
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int TotalTracks { get; set; }

        public List<TrackFinalRankingResponse> TrackRankings { get; set; } = new();

        public List<RankingResponse> EventTop3 { get; set; } = new();
    }
}
