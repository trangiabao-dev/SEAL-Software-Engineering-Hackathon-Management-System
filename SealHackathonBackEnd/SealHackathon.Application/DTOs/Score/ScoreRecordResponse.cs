using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Score
{
    public class ScoreRecordResponse
    {
        // ID của bản ghi vừa tạo
        public Guid Id { get; set; }

        // ID bài nộp được chấm
        public Guid SubmissionId { get; set; }

        // ID và tên Judge chấm
        public Guid JudgeId { get; set; }
        public string JudgeName { get; set; } = string.Empty;

        // ID và tên tiêu chí
        public int CriterionId { get; set; }
        public string CriterionName { get; set; } = string.Empty;

        // Điểm số và nhận xét
        public double Score { get; set; }
        public string? Comment { get; set; }

        // Chấm thử hay chấm thật
        public bool IsCalibration { get; set; }

        // Thời gian chấm
        public DateTime ScoredAt { get; set; }
    }
}