using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Submission;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    [ApiController]
    [Authorize]
    public class SubmissionController : BaseController
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // POST api/rounds/{roundId}/submissions
        [HttpPost("api/rounds/{roundId:int}/submissions")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> CreateSubmission(
            int roundId,
            [FromBody] CreateSubmissionRequest request)
        {
            var leaderId = GetCurrentAccountId();

            var result = await _submissionService.CreateSubmissionAsync(roundId, request, leaderId);

            return Ok(ApiResponse<SubmissionDto>.SuccessResult(result, "Nộp bài thành công."));
        }

        // PUT api/submissions/{id}
        [HttpPut("api/submissions/{id:guid}")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> UpdateSubmission(
            Guid id,
            [FromBody] UpdateSubmissionRequest request)
        {
            var leaderId = GetCurrentAccountId();

            var result = await _submissionService.UpdateSubmissionAsync(id, request, leaderId);

            return Ok(ApiResponse<SubmissionDto>.SuccessResult(result, "Cập nhật bài nộp thành công."));
        }

        // GET api/submissions/{id} - Xem chi tiết bài nộp.
        [HttpGet("api/submissions/{id:guid}")]
        [Authorize(Roles = $"{RoleConstants.Coordinator}," +
            $"{RoleConstants.Leader},{RoleConstants.Judge},{RoleConstants.Mentor}")]
        public async Task<IActionResult> GetSubmissionById(Guid id)
        {
            var accountId = GetCurrentAccountId();

            var result = await _submissionService.GetSubmissionByIdAsync(
                id, accountId,
                isCoordinator: User.IsInRole(RoleConstants.Coordinator),
                isJudge: User.IsInRole(RoleConstants.Judge),
                isMentor: User.IsInRole(RoleConstants.Mentor));

            return Ok(ApiResponse<SubmissionDto>.SuccessResult(result));
        }

        // GET api/teams/{teamId}/submissions - Xem bài nộp của Team.
        [HttpGet("api/teams/{teamId:guid}/submissions")]
        [Authorize(Roles = $"{RoleConstants.Coordinator}," +
            $"{RoleConstants.Leader},{RoleConstants.Mentor}")]
        public async Task<IActionResult> GetSubmissionsByTeam(Guid teamId)
        {
            var accountId = GetCurrentAccountId();

            var result = await _submissionService.GetSubmissionsByTeamAsync(
                teamId, accountId,
                isCoordinator: User.IsInRole(RoleConstants.Coordinator),
                isMentor: User.IsInRole(RoleConstants.Mentor));

            return Ok(ApiResponse<List<SubmissionDto>>.SuccessResult(result));
        }

        // GET api/rounds/{roundId}/submissions
        [HttpGet("api/rounds/{roundId:int}/submissions")]
        [Authorize(Roles = $"{RoleConstants.Coordinator},{RoleConstants.Judge}")]
        public async Task<IActionResult> GetSubmissionsByRound(int roundId)
        {
            var accountId = GetCurrentAccountId();

            var result = await _submissionService.GetSubmissionsByRoundAsync(roundId, accountId,
                User.IsInRole(RoleConstants.Coordinator),
                User.IsInRole(RoleConstants.Judge));

            return Ok(ApiResponse<List<SubmissionDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Import danh sách bài thi (Submissions) bằng Excel/CSV.
        /// Tự động cập nhật TopicId cho nhóm, tự động tạo RoundTeam (nếu cờ AutoCreateRoundTeam bật),
        /// và Upsert bài thi (thêm mới hoặc cập nhật PresentationUrl nếu đã có bài nộp).
        /// </summary>
        [HttpPost("api/admin/rounds/{roundId:int}/submissions/import")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> ImportSubmissions(int roundId, [FromBody] ImportSubmissionsRequest request)
        {
            var result = await _submissionService.ImportSubmissionsAsync(roundId, request);
            return Ok(result);
        }

        // PUT api/submissions/{id}/disqualify
        [HttpPut("api/submissions/{id:guid}/disqualify")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> DisqualifySubmission(
            Guid id,
            [FromBody] DisqualifySubmissionRequest request)
        {
            var coordinatorId = GetCurrentAccountId();

            await _submissionService.DisqualifySubmissionAsync(id, request, coordinatorId);

            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã loại bài nộp."));
        }
    }
}