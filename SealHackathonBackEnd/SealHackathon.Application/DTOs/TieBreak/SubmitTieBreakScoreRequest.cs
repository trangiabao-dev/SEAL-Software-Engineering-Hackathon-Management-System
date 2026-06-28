namespace SealHackathon.Application.DTOs.TieBreak
{
    /// <summary>
    /// Dữ liệu Judge gửi lên khi chấm lại một tiêu chí trong phiên tie-break.
    /// </summary>
    public class SubmitTieBreakScoreRequest
    {
        /// <summary>
        /// Tiêu chí đang được chấm lại trong phiên tie-break.
        /// </summary>
        public int CriterionId { get; set; }

        /// <summary>
        /// Điểm Judge nhập cho tiêu chí này.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Nhận xét của Judge khi chấm lại, không bắt buộc.
        /// </summary>
        public string? Comment { get; set; }
    }
}
