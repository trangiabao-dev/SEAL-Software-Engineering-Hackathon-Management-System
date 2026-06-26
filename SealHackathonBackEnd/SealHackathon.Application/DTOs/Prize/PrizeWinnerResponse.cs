namespace SealHackathon.Application.DTOs.Prize
{
    /// <summary>
    /// Dữ liệu đội đạt giải sau khi ghép cấu hình Prize với Ranking.
    /// </summary>
    public class PrizeWinnerResponse
    {
        public int PrizeId { get; set; }

        public string PrizeName { get; set; } = string.Empty;

        public string? PrizeDescription { get; set; }

        public int RankPosition { get; set; }

        public decimal? Amount { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int TrackId { get; set; }

        public string TrackName { get; set; } = string.Empty;

        public int RoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public Guid TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public double TotalScore { get; set; }

        public DateTime CalculatedAt { get; set; }
    }
}
