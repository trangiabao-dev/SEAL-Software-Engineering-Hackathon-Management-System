namespace SealHackathon.Application.DTOs.Score
{
    /// <summary>
    /// Thông tin một Submission trong lịch sử chấm của Judge hiện tại.
    /// </summary>
    public class JudgeScoreHistoryResponse
    {
        public Guid SubmissionId { get; set; }

        public Guid TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public string University { get; set; } = string.Empty;

        public int RoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        public DateTime LastScoredAt { get; set; }

        // Null nghĩa là Judge chưa chấm đủ tất cả criterion.
        public double? MyScore { get; set; }

        public int ScoredCriteriaCount { get; set; }

        public int TotalCriteriaCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
