namespace SealHackathon.Application.DTOs.Submission
{
    /// <summary>
    /// Dữ liệu đã chọn lọc để trả về cho FE hoặc nhận từ FE.
    /// </summary>
    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public int RoundId { get; set; }
        public string? DemoUrl { get; set; }
        public string? ReportUrl { get; set; }
        public string? AiEvaluation { get; set; }
        public bool IsDisqualified { get; set; }
        public string? DisqualifyReason { get; set; }
        public DateTime? DisqualifiedAt { get; set; }
        public Guid? DisqualifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
