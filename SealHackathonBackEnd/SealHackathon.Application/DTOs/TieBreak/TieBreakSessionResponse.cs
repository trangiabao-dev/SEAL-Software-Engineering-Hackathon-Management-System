namespace SealHackathon.Application.DTOs.TieBreak
{
    /// <summary>
    /// Dữ liệu một phiên tie-break, gồm thông tin Round và danh sách bài cần chấm lại.
    /// </summary>
    public class TieBreakSessionResponse
    {
        /// <summary>
        /// Id phiên tie-break.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id Round đang có nhóm đồng hạng.
        /// </summary>
        public int RoundId { get; set; }

        /// <summary>
        /// Tên Round để FE hiển thị.
        /// </summary>
        public string RoundName { get; set; } = string.Empty;

        /// <summary>
        /// Hạng đang bị đồng hạng, ví dụ 3 nghĩa là có nhiều đội cùng hạng 3.
        /// </summary>
        public int RankPosition { get; set; }

        /// <summary>
        /// Trạng thái phiên tie-break.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm tạo phiên tie-break.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời điểm hoàn tất phiên tie-break, nếu đã xong.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Danh sách bài nộp cần được chấm lại trong phiên này.
        /// </summary>
        public List<TieBreakSubmissionResponse> Submissions { get; set; } = new();

        /// <summary>
        /// Danh sách tiêu chí dùng để chấm điểm trong phiên này.
        /// </summary>
        public List<SealHackathon.Application.DTOs.Criteria.CriterionResponse> Criteria { get; set; } = new();
    }
}
