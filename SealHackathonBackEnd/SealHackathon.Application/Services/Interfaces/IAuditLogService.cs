namespace SealHackathon.Application.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task AddAsync(
            Guid performedBy, // Ai thực hiện hành động.
            string action, // Hành động gì.
            string entityName, // Tác động bảng/entity nào, ví dụ Team.
            string entityId, // Id dòng bị tác động.
            object? oldValues = null, // Dữ liệu trước khi đổi.
            object? newValues = null); // Dữ liệu sau khi đổi.
    }
}