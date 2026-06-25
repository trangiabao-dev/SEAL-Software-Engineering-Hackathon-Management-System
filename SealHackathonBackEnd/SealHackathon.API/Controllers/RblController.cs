using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Domain.Constants;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Rbl;
using SealHackathon.Application.Services.Interfaces;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace SealHackathon.API.Controllers
{
    /// <summary>
    /// Controller quản lý dữ liệu nghiên cứu (Research-Based Learning - RBL)
    /// </summary>
    [ApiController]
    [Route("api/rbl")]
    [Authorize]
    public class RblController : BaseController
    {
        private readonly IRblService _rblService;

        public RblController(IRblService rblService)
        {
            _rblService = rblService;
        }

        /// <summary>
        /// Xuất bộ dữ liệu chấm điểm đã ẩn danh (CSV) của sự kiện
        /// </summary>
        [HttpGet("events/{eventId:int}/export-anonymous")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> ExportAnonymousScoresCsv(int eventId)
        {
            var fileBytes = await _rblService.ExportAnonymousScoresCsvAsync(eventId);
            var fileName = $"rbl-anonymous-scores-event-{eventId}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(
                fileBytes,
                "text/csv",
                fileName);
        }

        /// <summary>
        /// Lấy phương sai điểm số chấm giữa các giám khảo theo từng tiêu chí trong sự kiện
        /// </summary>
        [HttpGet("events/{eventId:int}/criteria-variance")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetCriteriaVariance(int eventId)
        {
            var result = await _rblService.GetCriteriaVarianceAsync(eventId);
            return Ok(ApiResponse<List<CriterionVarianceResponse>>.SuccessResult(
                result, "Lấy dữ liệu phương sai điểm chấm thành công."));
        }
    }
}
