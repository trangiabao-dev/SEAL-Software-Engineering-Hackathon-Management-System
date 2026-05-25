using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Score
{
    public class SubmitScoreRequest
    {
        // Tiêu chí nào đang được chấm
        public int CriterionId { get; set; }

        // Điểm số Judge nhập vào
        public double Score { get; set; }

        // Nhận xét của Judge — không bắt buộc
        public string? Comment { get; set; }

        // Chấm thử để căn chỉnh — không tính vào điểm thật
        public bool IsCalibration { get; set; } = false;

        /* 
        - JudgeId — server tự lấy từ JWT token, không để client tự khai báo. Nếu để client tự nhập, Judge A có thể giả mạo thành Judge B.
        - SubmissionId — lấy từ URL, không cần trong body. API trông như thế này: POST /api/scores/submissions/{submissionId}
        - ScoredAt — server tự gán DateTime.UtcNow. Không để client tự nhập vì có thể gian lận thời gian.
        */
    }
}