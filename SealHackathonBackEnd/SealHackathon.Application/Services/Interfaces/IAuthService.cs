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
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string token);

        Task<List<AccountPendingResponse>> GetPendingAccountsAsync();
        Task ApproveAccountAsync(Guid accountId, Guid coordinatorId);
        Task RejectAccountAsync(Guid accountId, Guid coordinatorId, string reason);
        Task VerifyEmailAsync(string token);
    }
}
