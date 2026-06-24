namespace SealHackathon.Application.Services.Interfaces
{
    using SealHackathon.Application.Common.Responses;
    using SealHackathon.Application.DTOs.AuditLog;

    /// <summary>
    /// Cung cấp nghiệp vụ ghi và đọc lịch sử thay đổi dữ liệu cần kiểm tra.
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Thêm một bản ghi AuditLog vào Unit of Work hiện tại.
        /// Caller chịu trách nhiệm gọi SaveChangesAsync để lưu cùng dữ liệu nghiệp vụ.
        /// </summary>
        Task AddAsync(
            Guid performedBy, // Ai thực hiện hành động.
            string action, // Hành động gì.
            string entityName, // Tác động bảng/entity nào, ví dụ: nameof(Submission)
            string entityId, // Id dòng bị tác động -> submission.Id.ToString()
            object? oldValues = null, // Dữ liệu trước khi đổi.
            object? newValues = null); // Dữ liệu sau khi đổi.

        /// <summary>
        /// Lấy lịch sử tạo và sửa điểm của chính Judge đang đăng nhập.
        /// </summary>
        Task<PaginatedResponse<ScoreAuditLogResponse>> GetMyScoreAuditLogsAsync(
            Guid judgeId,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Lấy lịch sử tạo và sửa điểm của tất cả Judge cho Coordinator.
        /// Có thể lọc theo một Judge cụ thể.
        /// </summary>
        Task<PaginatedResponse<ScoreAuditLogResponse>> GetScoreAuditLogsAsync(
            Guid? judgeId,
            int pageNumber,
            int pageSize);
    }
}
