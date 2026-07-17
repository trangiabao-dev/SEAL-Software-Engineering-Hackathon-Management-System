namespace SealHackathon.Application.DTOs.TieBreak
{
    /// <summary>
    /// Dữ liệu một bài nộp cần được Judge chấm lại trong phiên tie-break.
    /// </summary>
    public class TieBreakSubmissionResponse
    {
        /// <summary>
        /// Id của dòng TieBreakSubmission, dùng khi Judge gửi điểm tie-break ở bước sau.
        /// </summary>
        public Guid TieBreakSubmissionId { get; set; }

        /// <summary>
        /// Id bài nộp gốc của đội.
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// Id đội sở hữu bài nộp.
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// Tên đội để FE hiển thị trong màn chấm lại.
        /// </summary>
        public string TeamName { get; set; } = string.Empty;

        /// <summary>
        /// Trường/đơn vị của đội.
        /// </summary>
        public string University { get; set; } = string.Empty;

        /// <summary>
        /// Link bài nộp/thuyết trình được chấm lại.
        /// </summary>
        public string PresentationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tổng điểm chấm lại của bài nộp này (nếu đã được chấm).
        /// </summary>
        public double? TotalTieBreakScore { get; set; }

        /// <summary>
        /// Hạng mới đạt được sau khi giải quyết đồng hạng (chỉ có khi phiên đã Completed).
        /// </summary>
        public int? FinalTieBreakRank { get; set; }
    }
}
