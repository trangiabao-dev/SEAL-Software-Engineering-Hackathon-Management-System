using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Track;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Track Service
    public interface ITrackService
    {
        Task<ApiResponse<List<TrackResponse>>> GetTracksByEventIdAsync(int eventId);
        Task<ApiResponse<TrackResponse>> CreateTrackAsync(CreateTrackRequest request);
        Task<ApiResponse<TrackResponse>> UpdateTrackAsync(int id, UpdateTrackRequest request);
        Task<ApiResponse<bool>> AssignMentorAsync(int trackId, AssignMentorRequest request, Guid assignedBy);
        Task<ApiResponse<MentorTeamAssignmentResponse>> AssignMentorToTeamsAsync(int trackId, Guid mentorId, AssignMentorTeamsRequest request, Guid assignedBy);
        Task<ApiResponse<bool>> AutoAssignMentorsAsync(int trackId, Guid assignedBy);
        Task<ApiResponse<MentorTeamAssignmentResponse>> GetMentorTeamsAsync(int trackId, Guid mentorId);
        Task<ApiResponse<List<TrackRoundsResponse>>> GetAllTracksWithRoundsAsync();
    }
}
