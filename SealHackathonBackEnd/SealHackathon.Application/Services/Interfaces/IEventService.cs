using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Event;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    // Giao diện (Interface) cho Event Service
    public interface IEventService
    {
        Task<ApiResponse<List<EventResponse>>> GetAllEventsAsync();
        Task<ApiResponse<EventResponse>> GetEventByIdAsync(int id);
        Task<ApiResponse<EventResponse>> CreateEventAsync(CreateEventRequest request);
        Task<ApiResponse<EventResponse>> UpdateEventAsync(int id, UpdateEventRequest request);
        Task<ApiResponse<bool>> DeleteEventAsync(int id); // Xóa mềm (Soft-delete)
    }
}
