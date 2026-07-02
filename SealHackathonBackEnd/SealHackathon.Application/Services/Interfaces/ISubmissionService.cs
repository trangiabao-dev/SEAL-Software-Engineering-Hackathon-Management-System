using SealHackathon.Application.DTOs.Submission;
using SealHackathon.Application.DTOs.Batch;
using SealHackathon.Application.Services.Interfaces;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface ISubmissionService
    {
        Task<SubmissionDto> CreateSubmissionAsync(int roundId,
            CreateSubmissionRequest request, Guid leaderId);
        Task<SubmissionDto> UpdateSubmissionAsync(Guid submissionId,
            UpdateSubmissionRequest request, Guid leaderId);

        /// <summary>
        /// Dùng để xem chi tiết một bài nộp. Leader/Judge/Coordinator/Mentor cần xem bài
        /// </summary>
        Task<SubmissionDto> GetSubmissionByIdAsync(Guid submissionId, 
            Guid currentAccountId, bool isCoordinator, bool isJudge, bool isMentor);

        /// <summary>
        /// Dùng để lấy danh sách bài nộp của một team. Team cần xem lịch sử nộp.
        /// </summary>
        Task<List<SubmissionDto>> GetSubmissionsByTeamAsync(Guid teamId, 
            Guid currentAccountId, bool isCoordinator, bool isMentor);

        /// <summary>
        /// Dùng để lấy danh sách bài nộp trong một Round.
        /// Hàm này cần cho Coordinator/Judge. Judge cần danh sách bài trong round để chấm điểm.
        /// </summary>
        Task<List<SubmissionDto>> GetSubmissionsByRoundAsync(int roundId, 
            Guid currentAccountId, bool isCoordinator, bool isJudge);
        /// <summary>
        /// Coordinator cần loại bài vi phạm
        /// </summary>
        Task DisqualifySubmissionAsync(Guid submissionId,
            DisqualifySubmissionRequest request, Guid coordinatorId);

        Task<BatchImportResponse<ImportSubmissionSuccessDto, ImportSubmissionSuccessDto>> ImportSubmissionsAsync(int roundId, ImportSubmissionsRequest request);
    }
}