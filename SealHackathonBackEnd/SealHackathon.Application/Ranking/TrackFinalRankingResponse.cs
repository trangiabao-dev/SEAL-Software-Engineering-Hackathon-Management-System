namespace SealHackathon.Application.DTOs.Ranking
{
    /// <summary>
    /// Bảng xếp hạng chính thức của vòng chung kết thuộc một Track.
    /// </summary>
    public class TrackFinalRankingResponse
    {
        public int TrackId { get; set; }

        public string TrackName { get; set; } = string.Empty;

        public RankingLeaderboardResponse FinalRoundRanking { get; set; } = new();
    }
}
