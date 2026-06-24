namespace SealHackathon.Application.DTOs.AuditLog
{
    /// <summary>
    /// Dữ liệu một lần tạo hoặc sửa điểm được trả về từ AuditLog.
    /// </summary>
    public class ScoreAuditLogResponse
    {
        public Guid AuditLogId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid? ScoreRecordId { get; set; }
        public Guid JudgeId { get; set; }
        public string JudgeName { get; set; } = string.Empty;
        public int? EventId { get; set; }
        public string? EventName { get; set; }
        public int? TrackId { get; set; }
        public string? TrackName { get; set; }
        public int? RoundId { get; set; }
        public string? RoundName { get; set; }
        public Guid? SubmissionId { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? University { get; set; }
        public int? CriterionId { get; set; }
        public string? CriterionName { get; set; }
        public double? OldScore { get; set; }
        public double? NewScore { get; set; }
        public string? OldComment { get; set; }
        public string? NewComment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
