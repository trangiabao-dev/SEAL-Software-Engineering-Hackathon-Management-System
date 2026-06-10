using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;

namespace SealHackathon.API.Controllers
{
    [ApiController] // Báo cho .NET biết đây là API. Check người dùng có gửi thiếu field bắt buộc trong DTO không
    [Route("api/teams")] // Quy định URL gốc cho toàn bộ API trong file này.
    [Authorize] // Thực thi cơ chế xác thực
    public class TeamController : BaseController
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // =======
        // LEADER
        // =======

        // POST api/teams — Leader tạo team
        [HttpPost] // Quy định API này chỉ nhận các Request gửi qua phương thức POST (chuyên dùng để tạo mới dữ liệu).
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
        {
            var leaderId = GetCurrentAccountId(); // // Lấy từ JWT Token — không thể giả mạo
            var result = await _teamService.CreateTeamAsync(request, leaderId);
            return Ok(ApiResponse<TeamDetailDto>.SuccessResult(result, "Tạo đội thành công. Chờ Coordinator duyệt."));
        }

        // GET api/teams/{id} — Lấy thông tin team
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdTeam(Guid id)
        {
            var result = await _teamService.GetByIdAsync(id);
            return Ok(ApiResponse<TeamDetailDto>.SuccessResult(result));
        }

        // PUT api/teams/{id} — Leader sửa team
        [HttpPut("{id:guid}")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request)
        {
            var leaderId = GetCurrentAccountId();
            var result = await _teamService.UpdateTeamAsync(id, request, leaderId);
            return Ok(ApiResponse<TeamDetailDto>.SuccessResult(result, "Cập nhật đội thành công."));
        }

        // POST api/teams/{id}/members — Leader thêm thành viên
        [HttpPost("{id:guid}/members")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request)
        {
            var leaderId = GetCurrentAccountId();
            var result = await _teamService.AddMemberAsync(id, request, leaderId);
            return Ok(ApiResponse<TeamMemberDto>.SuccessResult(result, "Thêm thành viên thành công."));
        }

        // PUT api/teams/{id}/members/{memberId} — Leader sửa thành viên
        [HttpPut("{id:guid}/members/{memberId:int}")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> UpdateMember(Guid id, int memberId, [FromBody] UpdateMemberRequest request)
        {
            var leaderId = GetCurrentAccountId();
            var result = await _teamService.UpdateMemberAsync(id, memberId, request, leaderId);
            return Ok(ApiResponse<TeamMemberDto>.SuccessResult(result, "Cập nhật thành viên thành công."));
        }

        // DELETE api/teams/{id}/members/{memberId} — Leader xóa thành viên
        [HttpDelete("{id:guid}/members/{memberId:int}")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> DeleteMember(Guid id, int memberId)
        {
            var leaderId = GetCurrentAccountId();
            await _teamService.DeleteMemberAsync(id, memberId, leaderId);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Xóa thành viên thành công."));
        }

        // GET api/teams/my-team — Leader lấy team của mình trong Event Registration hoặc Active hiện tại.
        [HttpGet("my-team")]
        [Authorize(Roles = RoleConstants.Leader)]
        public async Task<IActionResult> GetMyTeam()
        {
            var leaderId = GetCurrentAccountId();
            var result = await _teamService.GetMyTeamAsync(leaderId);

            return Ok(ApiResponse<TeamDetailDto?>.SuccessResult(result));
        }

        // ==========================================
        // COORDINATOR OPERATIONS
        // ==========================================

        // GET api/admin/teams — Coordinator xem tất cả team
        [HttpGet("/api/admin/teams")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> GetAllTeams(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] int? trackId = null)
        {
            var result = await _teamService.GetAllTeamsAsync(pageNumber, pageSize, status, trackId);
            return Ok(ApiResponse<PaginatedResponse<TeamListDto>>.SuccessResult(result));
        }

        // PUT api/teams/{id}/approve — Coordinator duyệt team
        [HttpPut("{id:guid}/approve")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> ApproveTeam(Guid id)
        {
            var coordinatorId = GetCurrentAccountId();
            await _teamService.ApproveTeamAsync(id, coordinatorId);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã duyệt đội thi thành công."));
        }

        // PUT api/teams/{id}/disqualify — Coordinator loại team
        [HttpPut("{id:guid}/disqualify")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> DisqualifyTeam(Guid id, [FromBody] DisqualifyTeamRequest request)
        {
            var coordinatorId = GetCurrentAccountId();

            await _teamService.DisqualifyTeamAsync(id, request, coordinatorId);

            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã loại đội thi."));
        }

        // PUT api/teams/{id}/mentor — Coordinator assign Mentor
        [HttpPut("{id:guid}/mentor")]
        [Authorize(Roles = RoleConstants.Coordinator)]
        public async Task<IActionResult> AssignMentor(Guid id, [FromBody] AssignMentorRequest request)
        {
            var coordinatorId = GetCurrentAccountId();
            await _teamService.AssignMentorAsync(id, request, coordinatorId);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Đã phân công Mentor thành công."));
        }
    }
}