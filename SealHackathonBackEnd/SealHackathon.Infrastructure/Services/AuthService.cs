using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SealHackathon.Application.DTOs.Auth;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SealHackathon.Infrastructure.Services;

public class AuthService : IAuthService
{
    // Các dependency (dịch vụ phụ thuộc) được tiêm (inject) vào để sử dụng
    private readonly IUnitOfWork _uow; // Dùng để tương tác với Database
    private readonly IEmailService _emailService; // Dùng để gửi email
    private readonly IConfiguration _config; // Dùng để đọc file appsettings.json

    // Lưu trữ danh sách các token đã đăng xuất (chặn không cho dùng lại)
    private static readonly HashSet<string> _blacklistedTokens = new();

    // Constructor: Khởi tạo các dịch vụ
    public AuthService(IUnitOfWork uow, IEmailService emailService, IConfiguration config)
    {
        _uow = uow;
        _emailService = emailService;
        _config = config;
    }

    // ==========================================
    // 1. ĐĂNG KÝ TÀI KHOẢN (REGISTER)
    // ==========================================
    public async Task RegisterAsync(RegisterRequest request)
    {
        var repo = _uow.GetRepository<Account>();

        // Bước 1: Kiểm tra xem Email do người dùng nhập đã tồn tại trong Database chưa
        var existingAccount = await repo.GetFirstOrDefaultAsync(a => a.Email == request.Email);

        // Tạo tự động Username từ Tên người dùng (Username) nhập vào
        // Loại bỏ dấu tiếng việt, viết thường và xóa khoảng trắng
        var baseUsername = RemoveVietnameseTone(request.Username).Replace(" ", "").ToLower();
        var finalUsername = baseUsername;
        bool isUnique = false;
        int counter = 1;

        // Vòng lặp kiểm tra xem Username này có ai xài chưa, nếu có rồi thì thêm số 1, 2, 3... vào đuôi
        while (!isUnique)
        {
            var userWithSameName = await repo.GetFirstOrDefaultAsync(a => a.Username == finalUsername);
            if (userWithSameName == null)
            {
                isUnique = true; // Không ai xài -> Hợp lệ
            }
            else
            {
                // Nếu bị trùng với account hiện tại (trong trường hợp Ghi đè Pending) thì không cần đếm lên
                if (existingAccount != null && userWithSameName.Id == existingAccount.Id)
                {
                    isUnique = true;
                }
                else
                {
                    finalUsername = $"{baseUsername}{counter}";
                    counter++;
                }
            }
        }

        var generatedUsername = finalUsername;
        Account account;

        if (existingAccount is not null)
        {
            // Nếu Email tồn tại nhưng KHÔNG phải trạng thái Pending (Đã được xác nhận hoặc là admin)
            if (existingAccount.SystemRole != "Pending")
            {
                throw new ConflictException("Email này đã được sử dụng. Vui lòng chọn email khác.");
            }
            else
            {
                // Nếu đang là "Pending", ta sẽ GHI ĐÈ dữ liệu mới để họ đăng ký lại (Cách các Ngân hàng cấp lại OTP)
                account = existingAccount;
                account.Username = generatedUsername;
                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                account.EmailConfirmToken = Guid.NewGuid().ToString();
                account.TokenExpiresAt = DateTime.UtcNow.AddMinutes(5); // Có thể chỉnh thành 5 phút (AddMinutes(5)) nếu muốn giống ngân hàng
                account.UpdatedAt = DateTime.UtcNow;

                repo.Update(account);
            }
        }
        else
        {
            // Bước 2: Tạo một đối tượng Account mới hoàn toàn
            account = new Account
            {
                Id = Guid.NewGuid(),
                Username = generatedUsername,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                SystemRole = "Pending",
                EmailConfirmToken = Guid.NewGuid().ToString(),
                TokenExpiresAt = DateTime.UtcNow.AddMinutes(5), // <-- SỬA Ở ĐÂY: Đổi thành 5 phút cho nhánh Tạo Mới
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(account);
        }

        // Lưu tài khoản (Mới hoặc Ghi đè) vào Database
        await _uow.SaveChangesAsync();

        // Link trỏ thẳng API BE — bấm email là xác nhận được (không cần FE proxy)
        var confirmationLink = BuildEmailVerificationLink(account.EmailConfirmToken!);

        var emailSubject = "Xác nhận đăng ký tài khoản FPT Hackathon 2026";
        // Nội dung Email được thiết kế dưới dạng HTML để có nút bấm đẹp mắt
        var emailBody = $@"
            <h3>Chào bạn,</h3>
            <p>Cảm ơn bạn đã đăng ký tham gia hệ thống giải đấu Seal Hackathon.</p>
            <p>Vui lòng click vào đường link bên dưới để xác minh địa chỉ email và kích hoạt tài khoản của bạn (Link có hiệu lực trong 5 phút):</p>
            <p><a href='{confirmationLink}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Xác nhận Email</a></p>
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>";

        // Gọi hàm từ EmailService để thực sự gửi mail đi
        await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
    }

    // ==========================================
    // 2. XÁC NHẬN EMAIL (VERIFY EMAIL)
    // ==========================================
    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new BadRequestException("Token xác nhận không hợp lệ.");

        var repo = _uow.GetRepository<Account>();

        // Phải dùng tracking — GetFirstOrDefaultAsync (AsNoTracking) + Update() có thể không ghi DB
        var account = await repo.GetFirstOrDefaultTrackingAsync(a => a.EmailConfirmToken == token);

        if (account == null)
            throw new BadRequestException("Token xác nhận không hợp lệ hoặc không tồn tại.");

        if (account.TokenExpiresAt is not null && account.TokenExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Link xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại email.");

        account.SystemRole = "Leader";
        account.EmailConfirmToken = null;
        account.TokenExpiresAt = null;
        account.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // 3. ĐĂNG NHẬP (LOGIN)
    // ==========================================
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var repo = _uow.GetRepository<Account>();

        // Tìm user bằng Email (bỏ qua những người đã bị xóa tài khoản)
        var account = await repo.GetFirstOrDefaultAsync(a => a.Email == request.Email && !a.IsDeleted);

        // Kiểm tra xem user có tồn tại không, VÀ mật khẩu nhập vào có khớp với mã Hash trong DB không
        if (account is null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            throw new BadRequestException("Email hoặc mật khẩu không đúng.");

        // Kiểm tra xem người này đã click vào link trong Email chưa (Role vẫn là Pending thì chặn lại)
        if (account.SystemRole == "Pending")
            throw new ForbiddenException("Tài khoản của bạn chưa được xác thực. Vui lòng kiểm tra hộp thư email của bạn để kích hoạt tài khoản.");

        // Bảo thêm cho Thức sửa lại rule Mentor và Judge
        if (account.SystemRole == RoleConstants.Inactive)
        {
            var activeEventAccount = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea =>
                    ea.AccountId == account.Id &&
                    ea.Status == "Approved" &&
                    !ea.Event.IsDeleted &&
                    ea.Event.Status == "Active");

            if (activeEventAccount is null)
                throw new ForbiddenException("Tài khoản này hiện không hoạt động trong sự kiện nào.");
        }

        // Nếu qua hết các vòng kiểm tra trên -> Tạo chìa khóa JWT cho người dùng
        var roles = await GetCurrentRolesAsync(account);
        var (token, expiresAt) = GenerateJwtToken(account, roles);

        return new LoginResponse
        {
            Token = token,
            Email = account.Email,
            Username = account.Username,
            SystemRole = account.SystemRole,
            Roles = roles,
            ExpiresAt = expiresAt
        };
    }

    // ==========================================
    // 4. ĐĂNG XUẤT (LOGOUT)
    // ==========================================
    public Task LogoutAsync(string token)
    {
        // Nhét token hiện tại vào danh sách đen để chặn mọi API dùng token này
        _blacklistedTokens.Add(token);
        return Task.CompletedTask;
    }

    // Hàm phụ trợ để kiểm tra token có nằm trong danh sách đen không
    public static bool IsTokenBlacklisted(string token) => _blacklistedTokens.Contains(token);

    // ==========================================
    // 5. CÁC HÀM CỦA QUẢN TRỊ VIÊN (COORDINATOR)
    // ==========================================
    // Lấy danh sách các tài khoản đang ở trạng thái Pending (Chưa duyệt/chưa xác thực)
    public async Task<List<AccountPendingResponse>> GetPendingAccountsAsync()
    {
        var repo = _uow.GetRepository<Account>();
        var pending = await repo.GetAllAsync(a => a.SystemRole == "Pending" && !a.IsDeleted);
        return pending.Select(a => new AccountPendingResponse { Id = a.Id, Username = a.Username, Email = a.Email, SystemRole = a.SystemRole, CreatedAt = a.CreatedAt }).ToList();
    }

    // Coordinator duyệt thủ công (Nâng role lên Leader)
    public async Task ApproveAccountAsync(Guid accountId, Guid coordinatorId)
    {
        var repo = _uow.GetRepository<Account>();
        var account = await repo.GetFirstOrDefaultTrackingAsync(a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted);
        if (account is null) throw new NotFoundException("Account", accountId);

        account.SystemRole = "Leader";
        account.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
    }

    // Coordinator từ chối tài khoản (Xóa mềm tài khoản đó)
    public async Task RejectAccountAsync(Guid accountId, Guid coordinatorId, string reason)
    {
        var repo = _uow.GetRepository<Account>();
        var account = await repo.GetFirstOrDefaultTrackingAsync(a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted);
        if (account is null) throw new NotFoundException("Account", accountId);

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // 6. HÀM TẠO CHÌA KHÓA (JWT TOKEN)
    // ==========================================

    // Bảo thêm cho Thức sửa lại rule Mentor và Judge
    private (string token, DateTime expiresAt) GenerateJwtToken(Account account, List<string> roles)
    {
        var jwt = _config.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpirationInMinutes"]!));

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, account.Email),
        new Claim("username", account.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenObj = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(tokenObj), expiresAt);
    }
    // ==========================================
    // 7. TẠO TÀI KHOẢN JUDGE/MENTOR (Dành cho Coordinator)
    // ==========================================

    // Bảo mới thêm vào đây
    public async Task<EventStaffResponse> CreateEventStaffAsync(int eventId, CreateEventStaffRequest request, Guid coordinatorId)
    {
        if (eventId <= 0)
            throw new BadRequestException("EventId không hợp lệ.");

        var allowedRoles = new[] { RoleConstants.Mentor, RoleConstants.Judge };
        if (!allowedRoles.Contains(request.EventRole))
            throw new BadRequestException("EventRole không hợp lệ. Chỉ hỗ trợ Mentor hoặc Judge.");

        if (request.EventRole == RoleConstants.Judge && string.IsNullOrWhiteSpace(request.JudgeType))
            throw new BadRequestException("JudgeType không được để trống khi tạo Judge.");

        if (request.EventRole == RoleConstants.Mentor && !string.IsNullOrWhiteSpace(request.JudgeType))
            throw new BadRequestException("Mentor không được có JudgeType.");

        var eventEntity = await _uow.GetRepository<Event>()
            .GetFirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);

        if (eventEntity is null)
            throw new NotFoundException("Event", eventId);

        var accountRepo = _uow.GetRepository<Account>();

        var account = await accountRepo
            .GetFirstOrDefaultTrackingAsync(a => a.Email == request.Email);

        var isNewAccount = false;
        var tempPassword = string.Empty;

        if (account is null)
        {
            isNewAccount = true;
            tempPassword = Guid.NewGuid().ToString("N")[..8];

            var normalizedName = RemoveVietnameseTone(request.Username).Replace(" ", "");
            var randomSuffix = Guid.NewGuid().ToString("N")[..4];

            account = new Account
            {
                Id = Guid.NewGuid(),
                Username = $"{normalizedName}_{randomSuffix}",
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                SystemRole = RoleConstants.Inactive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await accountRepo.AddAsync(account);
        }
        else
        {
            if (account.SystemRole != RoleConstants.Inactive)
                throw new BadRequestException("Email này đã thuộc tài khoản không phải staff. Không thể gán làm Mentor/Judge.");

            if (account.IsDeleted)
            {
                account.IsDeleted = false;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        var eventAccountRepo = _uow.GetRepository<EventAccount>();

        var eventAccount = await eventAccountRepo.GetFirstOrDefaultTrackingAsync(ea =>
            ea.EventId == eventId &&
            ea.AccountId == account.Id &&
            ea.EventRole == request.EventRole);

        if (eventAccount is null)
        {
            eventAccount = new EventAccount
            {
                EventId = eventId,
                AccountId = account.Id,
                EventRole = request.EventRole,
                JudgeType = request.EventRole == RoleConstants.Judge ? request.JudgeType : null,
                Status = "Approved",
                AssignedBy = coordinatorId,
                AssignedAt = DateTime.UtcNow
            };

            await eventAccountRepo.AddAsync(eventAccount);
        }
        else
        {
            eventAccount.JudgeType = request.EventRole == RoleConstants.Judge ? request.JudgeType : null;
            eventAccount.Status = "Approved";
            eventAccount.AssignedBy = coordinatorId;
            eventAccount.AssignedAt = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync();

        var loginLink = GetFrontendBaseUrl().TrimEnd('/') + "/login";

        if (isNewAccount)
        {
            var emailSubject = $"Thư mời tham gia SEAL Hackathon với vai trò {request.EventRole}";

            var emailBody = $@"
            <h3>Kính gửi {request.Username},</h3>
            <p>Bạn được mời tham gia sự kiện <strong>{eventEntity.Name}</strong> với vai trò <strong>{request.EventRole}</strong>.</p>
            <ul>
                <li><strong>Email đăng nhập:</strong> {request.Email}</li>
                <li><strong>Mật khẩu tạm thời:</strong> <span style='color:red;font-weight:bold;'>{tempPassword}</span></li>
            </ul>
            <p><a href='{loginLink}'>Đăng nhập hệ thống</a></p>";

            await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
        }
        else
        {
            var emailSubject = $"Bạn được phân công vai trò {request.EventRole} trong SEAL Hackathon";

            var emailBody = $@"
            <h3>Kính gửi {request.Username},</h3>
            <p>Tài khoản của bạn đã được phân công vai trò <strong>{request.EventRole}</strong> trong sự kiện <strong>{eventEntity.Name}</strong>.</p>
            <p>Vui lòng dùng tài khoản hiện có để đăng nhập.</p>
            <p><a href='{loginLink}'>Đăng nhập hệ thống</a></p>";

            await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
        }

        return new EventStaffResponse
        {
            AccountId = account.Id,
            EventId = eventId,
            Email = account.Email,
            EventRole = eventAccount.EventRole,
            Status = eventAccount.Status,
            JudgeType = eventAccount.JudgeType
        };
    }

    // Bảo mới thêm vào đây
    public async Task DeactivateEventStaffAsync(int eventId, Guid accountId, string eventRole, Guid coordinatorId)
    {
        if (eventId <= 0)
            throw new BadRequestException("EventId không hợp lệ.");

        if (accountId == Guid.Empty)
            throw new BadRequestException("AccountId không hợp lệ.");

        var allowedRoles = new[] { RoleConstants.Mentor, RoleConstants.Judge };
        if (!allowedRoles.Contains(eventRole))
            throw new BadRequestException("EventRole không hợp lệ. Chỉ hỗ trợ Mentor hoặc Judge.");

        var eventAccount = await _uow.GetRepository<EventAccount>()
            .GetFirstOrDefaultTrackingAsync(ea =>
                ea.EventId == eventId &&
                ea.AccountId == accountId &&
                ea.EventRole == eventRole);

        if (eventAccount is null)
            throw new NotFoundException("EventAccount", $"{eventId}-{accountId}");

        eventAccount.Status = "Inactive";
        eventAccount.AssignedBy = coordinatorId;
        eventAccount.AssignedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
    }

    private string BuildEmailVerificationLink(string confirmToken)
    {
        // Link mở trang FE — FE đọc token từ URL rồi gọi GET {ApiBaseUrl}/api/auth/verify-email?token=...
        return $"{GetFrontendBaseUrl()}/verify-email?token={Uri.EscapeDataString(confirmToken)}";
    }

    private string GetFrontendBaseUrl()
        => _config["AppSettings:FrontendBaseUrl"]?.TrimEnd('/')
           ?? "http://localhost:5173";

    // Hàm phụ trợ giúp xóa dấu tiếng Việt
    private string RemoveVietnameseTone(string text)
    {
        if (string.IsNullOrEmpty(text)) return "User";

        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    private async Task<List<string>> GetCurrentRolesAsync(Account account)
    {
        var roles = new List<string> { account.SystemRole };

        var activeEventRoles = await _uow.GetRepository<EventAccount>()
            .GetAllAsync(ea =>
                ea.AccountId == account.Id &&
                ea.Status == "Approved" &&
                !ea.Event.IsDeleted &&
                ea.Event.Status == "Active");

        foreach (var eventRole in activeEventRoles.Select(ea => ea.EventRole).Distinct())
        {
            roles.Add(eventRole);
        }

        return roles.Distinct().ToList();
    }
}
