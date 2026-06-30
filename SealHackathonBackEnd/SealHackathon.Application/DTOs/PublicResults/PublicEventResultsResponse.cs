namespace SealHackathon.Application.DTOs.PublicResults
{
    /// <summary>
    /// Kết quả công khai của một Event, dùng cho trang Results không cần đăng nhập.
    /// </summary>
    public class PublicEventResultsResponse
    {
        /// <summary>
        /// ID của Event.
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Tên Event.
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Tên Track Final chứa Final Round chung cuộc.
        /// </summary>
        public string FinalTrackName { get; set; } = string.Empty;

        /// <summary>
        /// Tên Final Round đã tạo Ranking chung cuộc.
        /// </summary>
        public string FinalRoundName { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm Ranking chung cuộc được tính.
        /// </summary>
        public DateTime? CalculatedAt { get; set; }

        /// <summary>
        /// Danh sách xếp hạng công khai của Final Round.
        /// </summary>
        public List<PublicRankingTeamResponse> Rankings { get; set; } = new();

        /// <summary>
        /// Danh sách đội đạt giải top 1, 2, 3.
        /// </summary>
        public List<PublicPrizeWinnerResponse> PrizeWinners { get; set; } = new();
    }
}
