using SealHackathon.Application.DTOs.Team;
using SealHackathon.Application.Common.Responses;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface ITeamService
    {
        // Leader
        Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, Guid leaderId);
        Task<TeamDetailDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid leaderId);
        Task<TeamDetailDto> GetByIdAsync(Guid teamId, 
            Guid currentAccountId, bool isCoordinator, bool isMentor);
        Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddMemberRequest request, Guid leaderId);
        Task<TeamMemberDto> UpdateMemberAsync(Guid teamId, int memberId, 
            UpdateMemberRequest request, Guid leaderId);
        Task DeleteMemberAsync(Guid teamId, int memberId, Guid leaderId);
        Task<TeamDetailDto?> GetMyTeamAsync(Guid leaderId);
        Task<MyActiveRoundResponse?> GetMyActiveRoundAsync(Guid leaderId);

        // Coordinator
        Task<PaginatedResponse<TeamListDto>> GetAllTeamsAsync(int pageNumber, int pageSize, string? status, int? trackId, int? eventId);
        Task<PaginatedResponse<ParticipantDto>> GetParticipantsAsync(int pageNumber, int pageSize, int? eventId, string? search);
        Task ApproveTeamAsync(Guid teamId, Guid coordinatorId);
        Task RejectTeamAsync(Guid teamId, RejectTeamRequest request, Guid coordinatorId);
        Task DisqualifyTeamAsync(Guid teamId, DisqualifyTeamRequest request, Guid coordinatorId);
        Task AssignMentorAsync(Guid teamId, AssignMentorRequest request, Guid coordinatorId);
        Task<TeamGroupedByStatusDto> GetTeamsGroupedByStatusAsync(int eventId, int? trackId);
        Task<List<TeamDetailDto>> MoveTeamsToTrackAsync(int targetTrackId, MoveTeamsToTrackRequest request, Guid coordinatorId);

        // Mentor
        Task<List<TeamDetailDto>> GetMyMentorTeamsAsync(Guid mentorId);
    }
}