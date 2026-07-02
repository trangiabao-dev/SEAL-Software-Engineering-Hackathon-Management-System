using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Score
{
    public class ImportScoresRequest
    {
        public List<ImportScoreDto> Scores { get; set; } = new List<ImportScoreDto>();
    }

    public class ImportScoreDto
    {
        public int RowNumber { get; set; }
        public Guid JudgeId { get; set; }
        public Guid SubmissionId { get; set; }
        public int CriterionId { get; set; }
        public double ScoreValue { get; set; }
        public string? Note { get; set; }
    }

    public class ImportScoreSuccessDto
    {
        public int RowNumber { get; set; }
        public Guid ScoreId { get; set; }
        public Guid SubmissionId { get; set; }
        public Guid JudgeId { get; set; }
        public int CriterionId { get; set; }
    }
}
