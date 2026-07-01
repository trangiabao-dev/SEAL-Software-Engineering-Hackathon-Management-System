namespace SealHackathon.Application.DTOs.PublicResults
{
    /// <summary>
    /// Dữ liệu xếp hạng công khai của một đội trong trang Results.
    /// </summary>
    public class PublicRankingTeamResponse
    {
        /// <summary>
        /// Tên đội được hiển thị công khai.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// Trường đại học của đội.
        /// </summary>
        public string University { get; set; } = string.Empty;

        /// <summary>
        /// Tổng điểm cuối cùng sau khi hệ thống đã tính Ranking.
        /// </summary>
        public double TotalScore { get; set; }

        /// <summary>
        /// Thứ hạng cuối cùng của đội trong Event.
        /// </summary>
        public int RankPosition { get; set; }
    }
}
