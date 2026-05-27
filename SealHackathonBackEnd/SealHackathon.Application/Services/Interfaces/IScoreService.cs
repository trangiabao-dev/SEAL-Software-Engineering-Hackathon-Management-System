using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SealHackathon.Application.DTOs.Score;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IScoreService
    {
        // Judge chấm điểm cho một submission
        // submissionId: lấy từ URL
        // judgeId: lấy từ JWT token
        // request: dữ liệu Judge nhập vào
        Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId,
            Guid judgeId,
            SubmitScoreRequest request);

        // Lấy danh sách điểm của một submission
        // Dùng để Judge xem lại điểm đã chấm
        Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(Guid submissionId);
    }
}
