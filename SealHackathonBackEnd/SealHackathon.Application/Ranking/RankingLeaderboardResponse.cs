namespace SealHackathon.Application.Ranking
{
    /// <summary>
    /// Wrapper DTO cho toàn bộ bảng xếp hạng của 1 round.
    /// Bao gồm metadata của round + danh sách ranking các đội.
    /// </summary>
    public class RankingLeaderboardResponse
    {
        /// <summary>ID vòng thi</summary>
        public int RoundId { get; set; }

        /// <summary>Tên vòng thi</summary>
        public string RoundName { get; set; } = string.Empty;

        /// <summary>Số slot vào vòng tiếp (null = vòng chung kết)</summary>
        public int? AdvancingSlots { get; set; }

        /// <summary>Tổng số đội trong bảng xếp hạng</summary>
        public int TotalTeams { get; set; }

        /// <summary>Thời điểm ranking được tính gần nhất</summary>
        public DateTime? CalculatedAt { get; set; }

        /// <summary>Danh sách ranking sắp xếp theo thứ hạng tăng dần</summary>
        public List<RankingResponse> Rankings { get; set; } = new();
    }
}
