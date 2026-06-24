using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.AuditLog;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Cung cấp API đọc lịch sử tạo và sửa điểm của Judge.
    /// </summary>
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize]
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogService _auditLogService;

        /// <summary>
        /// Khởi tạo controller đọc AuditLog.
        /// </summary>
        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Judge lấy lịch sử tạo và sửa điểm do chính mình thực hiện.
        /// </summary>
        [HttpGet("judges/scores")]
        [Authorize(Roles = RoleConstants.Judge)]
        public async Task<IActionResult> GetMyScoreAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var judgeId = GetCurrentAccountId();
            var result = await _auditLogService.GetMyScoreAuditLogsAsync(
                judgeId,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PaginatedResponse<ScoreAuditLogResponse>>
                .SuccessResult(result, "Lấy lịch sử thay đổi điểm thành công."));
        }

        /// <summary>
        /// Coordinator lấy lịch sử tạo và sửa điểm của tất cả Judge.
        /// </summary>
        [HttpGet("coordinators/scores")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetScoreAuditLogs(
            [FromQuery] Guid? judgeId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _auditLogService.GetScoreAuditLogsAsync(
                judgeId,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<PaginatedResponse<ScoreAuditLogResponse>>
                .SuccessResult(result, "Lấy AuditLog chấm điểm thành công."));
        }
    }
}
