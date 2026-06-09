using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Score
{
    /// <summary>
    /// Dữ liệu Judge gửi lên khi muốn sửa điểm đã chấm — PUT /api/scores/{scoreRecordId}
    /// </summary>
    public class UpdateScoreRequest
    {
        // Điểm mới mà Judge muốn sửa thành
        public double UpdatedScore { get; set; }

        // Nhận xét mới (không bắt buộc)
        public string? UpdatedComment { get; set; }
    }
}
