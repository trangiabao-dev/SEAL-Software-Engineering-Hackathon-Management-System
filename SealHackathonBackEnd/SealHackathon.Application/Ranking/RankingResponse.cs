namespace SealHackathon.Application.Ranking
{
    /// <summary>
    /// Dữ liệu ranking của 1 team trong 1 round — trả về cho client
    /// </summary> 
    public class RankingResponse
    {
        /// <summary>ID của bản ghi ranking</summary>
        public Guid Id { get; set; }

        /// <summary>ID của team</summary>
        public Guid TeamId { get; set; }

        /// <summary>Tên team</summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>ID vòng thi</summary>
        public int RoundId { get; set; }

        /// <summary>Tên vòng thi</summary>
        public string RoundName { get; set; } = string.Empty;

        /// <summary>Tổng điểm (weighted average)</summary>
        public double TotalScore { get; set; }

        /// <summary>Thứ hạng (1-indexed, hỗ trợ tie)</summary>
        public int RankPosition { get; set; }

        /// <summary>Có được vào vòng tiếp không</summary>
        public bool IsAdvancing { get; set; }

        /// <summary>Thời điểm ranking được tính</summary>
        public DateTime CalculatedAt { get; set; }
    }
}
