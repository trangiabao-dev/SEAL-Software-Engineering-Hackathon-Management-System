namespace SealHackathon.Application.Ranking
{
    /// <summary>
    /// Wrapper chứa toàn bộ bảng xếp hạng của 1 round 
    /// — bao gồm metadata và danh sách ranking các đội
    /// </summary>
    public class RankingLeaderboardResponse
    {
        /// ID vòng thi
        public int RoundId { get; set; }

        /// Tên vòng thi
        public string RoundName { get; set; } = string.Empty;

        /// Số slot vào vòng tiếp (null = vòng chung kết)
        public int? AdvancingSlots { get; set; }

        /// Tổng số đội trong bảng xếp hạng
        public int TotalTeams { get; set; }

        /// Thời điểm ranking được tính gần nhất
        public DateTime? CalculatedAt { get; set; }

        /// Danh sách ranking sắp xếp theo thứ hạng tăng dần
        public List<RankingResponse> Rankings { get; set; } = new();
    }
}
