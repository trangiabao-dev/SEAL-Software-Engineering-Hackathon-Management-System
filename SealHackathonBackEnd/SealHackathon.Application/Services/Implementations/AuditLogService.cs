using System.Text.Json;
using System.Linq.Expressions;
using SealHackathon.Application.Common.Requests;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.AuditLog;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Ghi AuditLog và cung cấp lịch sử tạo, sửa điểm cho Judge hoặc Coordinator.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private const int MaxActionLength = 100;
        private const int MaxEntityNameLength = 100;
        private const int MaxEntityIdLength = 100;

        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Khởi tạo service ghi và đọc AuditLog.
        /// </summary>
        public AuditLogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Thêm AuditLog vào Unit of Work hiện tại mà không tự gọi SaveChangesAsync.
        /// </summary>
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
        /// Lấy lịch sử tạo và sửa điểm của chính Judge đang đăng nhập.
        /// </summary>
        public Task<PaginatedResponse<ScoreAuditLogResponse>> GetMyScoreAuditLogsAsync(
            Guid judgeId,
            int pageNumber,
            int pageSize)
        {
            if (judgeId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidJudgeId);

            return GetScoreAuditLogsCoreAsync(judgeId, pageNumber, pageSize);
        }

        /// <summary>
        /// Lấy lịch sử tạo và sửa điểm của tất cả Judge cho Coordinator.
        /// </summary>
        public Task<PaginatedResponse<ScoreAuditLogResponse>> GetScoreAuditLogsAsync(
            Guid? judgeId,
            int pageNumber,
            int pageSize)
        {
            if (judgeId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidJudgeId);

            return GetScoreAuditLogsCoreAsync(judgeId, pageNumber, pageSize);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Dùng chung truy vấn AuditLog cho Judge và Coordinator.
        /// Judge truyền judgeId bắt buộc; Coordinator có thể để trống để xem toàn bộ.
        /// </summary>
        private async Task<PaginatedResponse<ScoreAuditLogResponse>> GetScoreAuditLogsCoreAsync(
            Guid? judgeId,
            int pageNumber,
            int pageSize)
        {
            ValidatePagination(pageNumber, pageSize);

            var auditLogRepository = _uow.GetRepository<AuditLog>();

            // Chỉ đọc log của ScoreRecord để không làm lộ AuditLog của module khác.
            Expression<Func<AuditLog, bool>> predicate = auditLog =>
                auditLog.EntityName == nameof(ScoreRecord)
                && (auditLog.Action == AuditActionConstants.ScoreAudit.Create
                    || auditLog.Action == AuditActionConstants.ScoreAudit.Update)
                && (!judgeId.HasValue || auditLog.PerformedBy == judgeId.Value);

            var totalRecords = await auditLogRepository.CountAsync(predicate);
            var auditLogs = await auditLogRepository.GetPagedAsync(
                predicate,
                auditLog => auditLog.CreatedAt,
                (pageNumber - 1) * pageSize,
                pageSize,
                descending: true);

            var items = await MapScoreAuditLogsAsync(auditLogs);

            return new PaginatedResponse<ScoreAuditLogResponse>(
                items,
                totalRecords,
                pageNumber,
                pageSize);
        }

        /// <summary>
        /// Ghép snapshot trong AuditLog với dữ liệu hiện tại để FE nhận được đầy đủ ngữ cảnh,
        /// đồng thời tải dữ liệu theo nhóm để tránh N+1 query.
        /// </summary>
        private async Task<List<ScoreAuditLogResponse>> MapScoreAuditLogsAsync(
            List<AuditLog> auditLogs)
        {
            if (auditLogs.Count == 0)
                return new List<ScoreAuditLogResponse>();

            var parsedLogs = auditLogs
                .Select(auditLog => new
                {
                    AuditLog = auditLog,
                    OldSnapshot = DeserializeScoreAuditSnapshot(auditLog.OldValues),
                    NewSnapshot = DeserializeScoreAuditSnapshot(auditLog.NewValues)
                })
                .ToList();

            var scoreRecordIds = auditLogs
                .Select(auditLog => Guid.TryParse(auditLog.EntityId, out var scoreRecordId)
                    ? scoreRecordId
                    : Guid.Empty)
                .Where(scoreRecordId => scoreRecordId != Guid.Empty)
                .Distinct()
                .ToList();

            var scoreRecords = scoreRecordIds.Count == 0
                ? new List<ScoreRecord>()
                : await _uow.GetRepository<ScoreRecord>()
                    .GetAllWithIncludeAsync(
                        scoreRecord => scoreRecordIds.Contains(scoreRecord.Id),
                        scoreRecord => scoreRecord.Judge,
                        scoreRecord => scoreRecord.Criterion,
                        scoreRecord => scoreRecord.Submission.Team,
                        scoreRecord => scoreRecord.Submission.Round.Track.Event);

            var scoreRecordById = scoreRecords.ToDictionary(item => item.Id);

            return parsedLogs
                .Select(item => MapToScoreAuditLogResponse(
                    item.AuditLog,
                    item.OldSnapshot,
                    item.NewSnapshot,
                    scoreRecordById))
                .ToList();
        }

        /// <summary>
        /// Chuyển một AuditLog và dữ liệu liên quan thành response cho FE.
        /// </summary>
        private static ScoreAuditLogResponse MapToScoreAuditLogResponse(
            AuditLog auditLog,
            ScoreAuditSnapshot? oldSnapshot,
            ScoreAuditSnapshot? newSnapshot,
            IReadOnlyDictionary<Guid, ScoreRecord> scoreRecordById)
        {
            var snapshot = newSnapshot ?? oldSnapshot;

            var scoreRecordId = Guid.TryParse(auditLog.EntityId, out var parsedScoreRecordId)
                ? parsedScoreRecordId
                : (Guid?)null;

            ScoreRecord? scoreRecord = null;
            if (scoreRecordId.HasValue)
                scoreRecordById.TryGetValue(scoreRecordId.Value, out scoreRecord);

            var submission = scoreRecord?.Submission;
            var team = submission?.Team;
            var round = submission?.Round;
            var track = round?.Track;
            var eventEntity = track?.Event;
            var criterion = scoreRecord?.Criterion;

            return new ScoreAuditLogResponse
            {
                AuditLogId = auditLog.Id,
                Action = auditLog.Action,
                ScoreRecordId = scoreRecordId,
                JudgeId = auditLog.PerformedBy,
                JudgeName = scoreRecord?.Judge.Username ?? string.Empty,
                EventId = eventEntity?.Id,
                EventName = eventEntity?.Name,
                TrackId = track?.Id,
                TrackName = track?.Name,
                RoundId = round?.Id,
                RoundName = round?.Name,
                SubmissionId = snapshot?.SubmissionId ?? scoreRecord?.SubmissionId,
                TeamId = team?.Id,
                TeamName = team?.TeamName,
                University = team?.University,
                CriterionId = snapshot?.CriterionId ?? scoreRecord?.CriterionId,
                CriterionName = criterion?.Name,
                OldScore = oldSnapshot?.Score,
                NewScore = newSnapshot?.Score,
                OldComment = oldSnapshot?.Comment,
                NewComment = newSnapshot?.Comment,
                CreatedAt = auditLog.CreatedAt
            };
        }

        /// <summary>
        /// Kiểm tra tham số phân trang trước khi query database.
        /// </summary>
        private static void ValidatePagination(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageNumber);

            if (pageSize < 1 || pageSize > PaginationRequest.MaxPageSize)
                throw new BadRequestException(ErrorMessages.Common.InvalidPageSize);
        }

        /// <summary>
        /// Đọc snapshot điểm đã được ScoreService lưu dưới dạng JSON.
        /// </summary>
        private static ScoreAuditSnapshot? DeserializeScoreAuditSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<ScoreAuditSnapshot>(json, JsonOptions);
        }

        /// <summary>
        /// Chuyển oldValues hoặc newValues thành chuỗi JSON trước khi lưu database.
        /// </summary>
        private static string? ToJson(object? value)
        {
            return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
        }

        /// <summary>
        /// Kiểm tra chuỗi bắt buộc trước khi lưu AuditLog.
        /// </summary>
        private static string ValidateRequiredText(string value, string parameterName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống!", parameterName);

            value = value.Trim();

            if (value.Length > maxLength)
                throw new ArgumentException($"{parameterName} không được quá {maxLength} ký tự!", parameterName);

            return value;
        }

        /// <summary>
        /// Cấu trúc snapshot tối thiểu mà ScoreService lưu trong OldValues và NewValues.
        /// </summary>
        private sealed class ScoreAuditSnapshot
        {
            public Guid SubmissionId { get; set; }

            public Guid JudgeId { get; set; }

            public int CriterionId { get; set; }

            public double Score { get; set; }

            public string? Comment { get; set; }
        }
    }
}
