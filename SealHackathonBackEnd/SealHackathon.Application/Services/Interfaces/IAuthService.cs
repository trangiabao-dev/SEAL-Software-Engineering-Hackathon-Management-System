using SealHackathon.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task VerifyEmailAsync(string token);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string token);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);

        // Bảo Thêm
        Task<EventStaffResponse> CreateEventStaffAsync(int eventId, CreateEventStaffRequest request, Guid coordinatorId);
        Task DeactivateEventStaffAsync(int eventId, Guid accountId, string eventRole, Guid coordinatorId);
        Task ActivateEventRoleAsync(int eventId, Guid accountId, string eventRole, Guid coordinatorId);
        Task<List<EventStaffResponse>> GetEventStaffAsync(int eventId);
    }
}
