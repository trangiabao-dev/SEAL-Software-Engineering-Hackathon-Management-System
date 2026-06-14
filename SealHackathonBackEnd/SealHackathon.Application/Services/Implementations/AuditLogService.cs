using System.Text.Json;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
namespace SealHackathon.Application.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private const int MaxActionLength = 100;
        private const int MaxEntityNameLength = 100;
        private const int MaxEntityIdLength = 100;

        private readonly IUnitOfWork _uow;

        public AuditLogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task AddAsync(Guid performedBy, string action, string entityName,
            string entityId, object? oldValues = null, object? newValues = null)
        {
            // Kiểm tra: người thực hiện hành động audit log có hợp lệ không.
            if (performedBy == Guid.Empty)
                throw new ArgumentException("PerformedBy không hợp lệ.", nameof(performedBy));

            action = ValidateRequiredText(action, nameof(action), MaxActionLength);
            entityName = ValidateRequiredText(entityName, nameof(entityName), MaxEntityNameLength);
            entityId = ValidateRequiredText(entityId, nameof(entityId), MaxEntityIdLength);

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                PerformedBy = performedBy,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = ToJson(oldValues),
                NewValues = ToJson(newValues),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GetRepository<AuditLog>().AddAsync(auditLog);
        }

        /// <summary>
        /// Hàm này dùng để chuyển oldValues hoặc newValues thành chuỗi JSON trước khi lưu vào database.
        /// </summary>
        private static string? ToJson(object? value)
        {
            return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
        }

        private static string ValidateRequiredText(string value, string parameterName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống!", parameterName);

            value = value.Trim();

            if (value.Length > maxLength)
                throw new ArgumentException($"{parameterName} không được quá {maxLength} ký tự!", parameterName);

            return value;
        }
    }
}
