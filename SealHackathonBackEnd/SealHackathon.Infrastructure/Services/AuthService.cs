using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SealHackathon.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private static readonly HashSet<string> _blacklistedTokens = new();

    public AuthService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
    }

    // ==========================================
    // REGISTER
    // ==========================================
    public async Task RegisterAsync(RegisterRequest request)
    {
        var repo = _uow.GetRepository<Account>();

        var existingEmail = await repo.GetFirstOrDefaultAsync(
            a => a.Email == request.Email
        );
        if (existingEmail is not null)
            throw new ConflictException("Email này đã được sử dụng.");

        var existingUsername = await repo.GetFirstOrDefaultAsync(
            a => a.Username == request.Username
        );
        if (existingUsername is not null)
            throw new ConflictException("Username này đã được sử dụng.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            SystemRole = "Pending", // Chờ Coordinator duyệt
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(account);
        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // LOGIN
    // ==========================================
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var repo = _uow.GetRepository<Account>();

        var account = await repo.GetFirstOrDefaultAsync(
            a => a.Email == request.Email && !a.IsDeleted
        );

        if (account is null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            throw new BadRequestException("Email hoặc mật khẩu không đúng.");

        // Chặn account chưa được duyệt
        if (account.SystemRole == "Pending")
            throw new ForbiddenException("Tài khoản của bạn đang chờ Coordinator duyệt.");

        var (token, expiresAt) = GenerateJwtToken(account);

        return new LoginResponse
        {
            Token = token,
            Email = account.Email,
            Username = account.Username,
            SystemRole = account.SystemRole,
            ExpiresAt = expiresAt
        };
    }

    // ==========================================
    // LOGOUT
    // ==========================================
    public Task LogoutAsync(string token)
    {
        _blacklistedTokens.Add(token);
        return Task.CompletedTask;
    }

    public static bool IsTokenBlacklisted(string token)
        => _blacklistedTokens.Contains(token);

    // ==========================================
    // GET PENDING ACCOUNTS (Coordinator)
    // ==========================================
    public async Task<List<AccountPendingResponse>> GetPendingAccountsAsync()
    {
        var repo = _uow.GetRepository<Account>();

        var pending = await repo.GetAllAsync(
            a => a.SystemRole == "Pending" && !a.IsDeleted
        );

        return pending.Select(a => new AccountPendingResponse
        {
            Id = a.Id,
            Username = a.Username,
            Email = a.Email,
            SystemRole = a.SystemRole,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    // ==========================================
    // APPROVE ACCOUNT (Coordinator)
    // ==========================================
    public async Task ApproveAccountAsync(Guid accountId, Guid coordinatorId)
    {
        var repo = _uow.GetRepository<Account>();

        var account = await repo.GetFirstOrDefaultAsync(
            a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted
        );

        if (account is null)
            throw new NotFoundException("Account", accountId);

        account.SystemRole = "Participant";
        account.UpdatedAt = DateTime.UtcNow;

        repo.Update(account);
        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // REJECT ACCOUNT (Coordinator)
    // ==========================================
    public async Task RejectAccountAsync(Guid accountId, Guid coordinatorId, string reason)
    {
        var repo = _uow.GetRepository<Account>();

        var account = await repo.GetFirstOrDefaultAsync(
            a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted
        );

        if (account is null)
            throw new NotFoundException("Account", accountId);

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;

        repo.Update(account);
        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // GENERATE JWT TOKEN
    // ==========================================
    private (string token, DateTime expiresAt) GenerateJwtToken(Account account)
    {
        var jwt = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpirationInMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim("username",                    account.Username),
            new Claim(ClaimTypes.Role,               account.SystemRole),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var tokenObj = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(tokenObj), expiresAt);
    }
}