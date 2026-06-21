using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Round;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Round Service
    public interface IRoundService
    {
        Task<ApiResponse<List<RoundResponse>>> GetRoundsByTrackIdAsync(int trackId);
        Task<ApiResponse<List<RoundSelectionResponse>>> GetRoundsForSelectionByEventAsync(int eventId);
        Task<ApiResponse<RoundResponse>> CreateRoundAsync(CreateRoundRequest request);
        Task<ApiResponse<RoundResponse>> UpdateRoundAsync(int id, UpdateRoundRequest request);
        Task<ApiResponse<RoundResponse>> UpdateRoundStatusAsync(int id, UpdateRoundStatusRequest request); // Sẽ xử lý Rule 7 ở đây
        Task<ApiResponse<bool>> AssignJudgeAsync(int roundId, AssignJudgeRequest request, Guid assignedBy);
        Task<ApiResponse<List<RoundJudgeResponse>>> GetJudgesByRoundAsync(int roundId);
        Task<ApiResponse<List<JudgeAssignedRoundResponse>>> GetAssignedRoundsForJudgeAsync(Guid judgeId);
    }
}
