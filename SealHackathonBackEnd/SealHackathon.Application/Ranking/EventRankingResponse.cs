namespace SealHackathon.Application.DTOs.Ranking
{
    /// <summary>
    /// Tổng hợp bảng xếp hạng chung kết của tất cả Track trong Event.
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
