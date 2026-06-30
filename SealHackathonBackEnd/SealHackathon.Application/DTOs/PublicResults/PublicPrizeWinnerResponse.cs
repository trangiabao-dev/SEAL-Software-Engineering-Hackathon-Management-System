namespace SealHackathon.Application.DTOs.PublicResults
{
    /// <summary>
    /// Dữ liệu đội đạt giải được phép công bố trên trang Results.
    /// </summary>
    public class PublicPrizeWinnerResponse
    {
        /// <summary>
        /// Tên giải thưởng, ví dụ Giải Nhất, Giải Nhì, Giải Ba.
        /// </summary>
        public string PrizeName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả giải thưởng nếu có.
        /// </summary>
        public string? PrizeDescription { get; set; }

        /// <summary>
        /// Vị trí nhận giải, ví dụ 1, 2 hoặc 3.
        /// </summary>
        public int RankPosition { get; set; }

        /// <summary>
        /// Số tiền thưởng nếu Event có cấu hình.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Tên đội nhận giải.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// Trường đại học của đội nhận giải.
        /// </summary>
        public string University { get; set; } = string.Empty;

        /// <summary>
        /// Tổng điểm cuối cùng của đội nhận giải.
        /// </summary>
        public double TotalScore { get; set; }
    }
}
