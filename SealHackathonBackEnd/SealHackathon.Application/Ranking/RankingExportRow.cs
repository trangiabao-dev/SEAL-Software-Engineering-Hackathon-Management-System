namespace SealHackathon.Application.DTOs.Ranking
{
    /// <summary>
    /// Một dòng dữ liệu trong báo cáo Ranking XLSX.
    /// </summary>
    public class RankingExportRow
    {
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

        public int RankPosition { get; set; }

        public bool IsAdvancing { get; set; }

        public DateTime CalculatedAt { get; set; }
    }
}
