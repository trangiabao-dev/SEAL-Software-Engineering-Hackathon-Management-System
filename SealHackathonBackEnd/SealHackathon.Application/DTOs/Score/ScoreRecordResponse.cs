namespace SealHackathon.Application.DTOs.Score
{
    /// <summary>
    /// Dữ liệu trả về cho client sau khi chấm điểm hoặc khi xem danh sách điểm của một Submission
    /// </summary>
    public class ScoreRecordResponse
    {
        public Guid Id { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid JudgeId { get; set; }
        public string JudgeName { get; set; } = string.Empty;
        public int CriterionId { get; set; }
        public string CriterionName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string? Comment { get; set; }
        public bool IsCalibration { get; set; }
        public DateTime ScoredAt { get; set; }
    }
}