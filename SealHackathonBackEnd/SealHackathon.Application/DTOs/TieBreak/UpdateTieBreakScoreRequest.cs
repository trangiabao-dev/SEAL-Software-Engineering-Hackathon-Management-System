namespace SealHackathon.Application.DTOs.TieBreak
{
    /// <summary>
    /// Dữ liệu Judge gửi lên khi sửa điểm đã chấm trong phiên tie-break.
    /// </summary>
    public class UpdateTieBreakScoreRequest
    {
        /// <summary>
        /// Điểm mới mà Judge muốn cập nhật.
        /// </summary>
        public double UpdatedScore { get; set; }

        /// <summary>
        /// Nhận xét mới sau khi cập nhật, không bắt buộc.
        /// </summary>
        public string? UpdatedComment { get; set; }
    }
}
