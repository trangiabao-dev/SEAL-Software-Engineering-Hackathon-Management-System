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

        Task CreateAccountByCoordinatorAsync(CreateAccountRequest request, Guid coordinatorId);
    }
}
