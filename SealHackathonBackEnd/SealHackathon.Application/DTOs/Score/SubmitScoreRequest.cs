using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Score
{
    /// <summary>
    /// Dữ liệu Judge gửi lên khi chấm điểm lần đầu cho một Submission — POST /api/scores/submissions/{submissionId}
    /// </summary>
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
    }
}