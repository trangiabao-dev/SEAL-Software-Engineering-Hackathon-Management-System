namespace SealHackathon.Application.DTOs.TieBreak
{
    /// <summary>
    /// Dữ liệu điểm tie-break trả về cho FE sau khi Judge chấm hoặc sửa điểm.
    /// </summary>
    public class TieBreakScoreResponse
    {
        public Guid Id { get; set; }

        public Guid TieBreakSubmissionId { get; set; }

        public Guid JudgeId { get; set; }

        public string JudgeName { get; set; } = string.Empty;

        public int CriterionId { get; set; }

        public string CriterionName { get; set; } = string.Empty;

        public double Score { get; set; }

        public string? Comment { get; set; }

        public DateTime ScoredAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
