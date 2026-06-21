using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Score;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IScoreService
    {
        /// <summary>
        /// Judge chấm điểm cho một Submission — tạo mới ScoreRecord trong DB
        /// </summary>
        Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId,
            Guid judgeId,
            SubmitScoreRequest request);

        // Lấy danh sách điểm của một submission
        // Dùng để Judge xem lại điểm đã chấm
        Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(
            Guid submissionId, Guid currentAccountId, bool isCoordinator);

        /// <summary>
        /// Lấy lịch sử chấm bài có phân trang của Judge hiện tại.
        /// </summary>
        Task<PaginatedResponse<JudgeScoreHistoryResponse>> GetMyScoreHistoryAsync(
            Guid judgeId,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Judge sửa điểm đã chấm — cập nhật ScoreRecord trong DB
        /// </summary>
        Task<ScoreRecordResponse> UpdateScoreAsync(
            Guid scoreRecordId,
            Guid judgeId,
            UpdateScoreRequest request);


    }
}
