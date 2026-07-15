using SealHackathon.Application.DTOs.TieBreak;

namespace SealHackathon.Application.Services.Interfaces
{
    /// <summary>
    /// Xử lý nghiệp vụ tạo và đọc phiên chấm lại tie-break.
    /// </summary>
    public interface ITieBreakService
    {
        /// <summary>
        /// Coordinator tạo phiên tie-break cho một nhóm đội đang đồng hạng trong Round.
        /// </summary>
        Task<TieBreakSessionResponse> CreateSessionAsync(int roundId, int rankPosition);

        /// <summary>
        /// Tự tạo phiên tie-break nếu hạng đồng hạng quan trọng chưa có phiên đang chờ chấm.
        /// </summary>
        Task<TieBreakSessionResponse?> CreateSessionIfNotExistsAsync(int roundId, int rankPosition);

        /// <summary>
        /// Judge lấy danh sách phiên tie-break đang chờ chấm của các Round mình được phân công.
        /// </summary>
        Task<List<TieBreakSessionResponse>> GetMyPendingSessionsAsync(Guid judgeId);

        /// <summary>
        /// Coordinator lấy danh sách toàn bộ các phiên tie-break của một Round.
        /// </summary>
        Task<List<TieBreakSessionResponse>> GetSessionsByRoundAsync(int roundId);

        /// <summary>
        /// Lấy chi tiết một phiên tie-break.
        /// Coordinator được xem mọi phiên; Judge chỉ được xem phiên thuộc Round mình được phân công.
        /// </summary>
        Task<TieBreakSessionResponse> GetSessionAsync(
            Guid sessionId,
            Guid currentAccountId,
            bool isCoordinator);

        /// <summary>
        /// Lấy danh sách điểm tie-break của một bài trong phiên chấm lại.
        /// Coordinator xem tất cả điểm; Judge chỉ xem điểm do chính mình chấm.
        /// </summary>
        Task<List<TieBreakScoreResponse>> GetScoresByTieBreakSubmissionAsync(
            Guid tieBreakSubmissionId,
            Guid currentAccountId,
            bool isCoordinator);

        /// <summary>
        /// Judge chấm một tiêu chí cho một bài trong phiên tie-break.
        /// </summary>
        Task<TieBreakScoreResponse> SubmitScoreAsync(
            Guid tieBreakSubmissionId,
            Guid judgeId,
            SubmitTieBreakScoreRequest request);

        /// <summary>
        /// Judge chấm nhiều tiêu chí cùng lúc cho một bài trong phiên tie-break (Bulk Submit).
        /// </summary>
        Task<List<TieBreakScoreResponse>> SubmitScoresAsync(
            Guid tieBreakSubmissionId,
            Guid judgeId,
            List<SubmitTieBreakScoreRequest> requests);

        /// <summary>
        /// Judge sửa điểm tie-break do chính mình đã chấm.
        /// </summary>
        Task<TieBreakScoreResponse> UpdateScoreAsync(
            Guid tieBreakScoreRecordId,
            Guid judgeId,
            UpdateTieBreakScoreRequest request);

        /// <summary>
        /// Coordinator tính kết quả tie-break và cập nhật lại thứ hạng trong bảng Ranking.
        /// </summary>
        Task<TieBreakSessionResponse> CalculateResultAsync(Guid sessionId);

        /// <summary>
        /// Lấy thứ tự xếp hạng (đã phân định xong) của tất cả các phiên tie-break đã Completed trong một Round.
        /// Dictionary trả về có Key là RankPosition gốc, Value là danh sách TeamId đã được xếp hạng từ cao xuống thấp.
        /// </summary>
        Task<Dictionary<int, List<Guid>>> GetCompletedTieBreakOrdersAsync(int roundId);
    }
}
