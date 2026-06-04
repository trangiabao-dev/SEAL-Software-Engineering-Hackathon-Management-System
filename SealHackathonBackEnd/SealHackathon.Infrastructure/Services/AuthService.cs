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
        var existingEmail = await repo.GetFirstOrDefaultAsync(a => a.Email == request.Email);
        if (existingEmail is not null)
            throw new ConflictException("Email này đã được sử dụng. Vui lòng chọn email khác.");

        // Bước 2: Tự động tạo Username từ Email để tránh lỗi trùng lặp Username trong DB
        // Cắt lấy phần trước chữ @ và cộng thêm 4 ký tự ngẫu nhiên
        var generatedUsername = request.Email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N")[..4];

        // Bước 3: Tạo một đối tượng Account mới để chuẩn bị lưu vào Database
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = generatedUsername,
            Email = request.Email,
            // Mã hóa mật khẩu bằng BCrypt trước khi lưu để bảo mật
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

            // Đặt Role tạm thời là "Pending" (Chờ xác thực)
            SystemRole = "Pending",
            // Tạo ra một chuỗi mã ngẫu nhiên để làm Link xác nhận
            EmailConfirmToken = Guid.NewGuid().ToString(),
            // Đặt thời gian hết hạn cho link là 24 giờ tính từ hiện tại
            TokenExpiresAt = DateTime.UtcNow.AddHours(24),

            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Lưu tài khoản mới vào Database
        await repo.AddAsync(account);
        await _uow.SaveChangesAsync();

        // Bước 4: Tạo link xác nhận và gửi Email cho người dùng
        // Lưu ý: Port 3000 là port mặc định của React. 
        var confirmationLink = $"http://localhost:3000/api/auth/verify-email?token={account.EmailConfirmToken}";

        var emailSubject = "Xác nhận đăng ký tài khoản FPT Hackathon 2026";
        // Nội dung Email được thiết kế dưới dạng HTML để có nút bấm đẹp mắt
        var emailBody = $@"
            <h3>Chào bạn,</h3>
            <p>Cảm ơn bạn đã đăng ký tham gia hệ thống giải đấu Seal Hackathon.</p>
            <p>Vui lòng click vào đường link bên dưới để xác minh địa chỉ email và kích hoạt tài khoản của bạn (Link có hiệu lực trong 24 giờ):</p>
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
        var repo = _uow.GetRepository<Account>();

        // Bước 1: Tìm xem có tài khoản nào đang giữ cái mã Token này không
        var account = await repo.GetFirstOrDefaultAsync(a => a.EmailConfirmToken == token);

        // Nếu không tìm thấy (token ảo) thì báo lỗi
        if (account == null)
            throw new BadRequestException("Token xác nhận không hợp lệ hoặc không tồn tại.");

        // Bước 2: Kiểm tra xem Link này đã quá hạn (24h) chưa
        if (account.TokenExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Link xác nhận đã hết hạn. Vui lòng yêu cầu gửi lại email.");

        // Bước 3: Nếu mọi thứ OK -> Nâng cấp Role cho người dùng
        account.SystemRole = "Leader"; // Trở thành thí sinh chính thức

        // Xóa mã Token đi để người dùng không thể bấm lại link này lần 2
        account.EmailConfirmToken = null;
        account.TokenExpiresAt = null;
        account.UpdatedAt = DateTime.UtcNow;

        // Cập nhật Database
        repo.Update(account);
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

        // Nếu qua hết các vòng kiểm tra trên -> Tạo chìa khóa JWT cho người dùng
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
        var account = await repo.GetFirstOrDefaultAsync(a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted);
        if (account is null) throw new NotFoundException("Account", accountId);

        account.SystemRole = "Leader";
        account.UpdatedAt = DateTime.UtcNow;
        repo.Update(account);
        await _uow.SaveChangesAsync();
    }

    // Coordinator từ chối tài khoản (Xóa mềm tài khoản đó)
    public async Task RejectAccountAsync(Guid accountId, Guid coordinatorId, string reason)
    {
        var repo = _uow.GetRepository<Account>();
        var account = await repo.GetFirstOrDefaultAsync(a => a.Id == accountId && a.SystemRole == "Pending" && !a.IsDeleted);
        if (account is null) throw new NotFoundException("Account", accountId);

        account.IsDeleted = true; // Đánh dấu là đã xóa (Xóa mềm)
        account.UpdatedAt = DateTime.UtcNow;
        repo.Update(account);
        await _uow.SaveChangesAsync();
    }

    // ==========================================
    // 6. HÀM TẠO CHÌA KHÓA (JWT TOKEN)
    // ==========================================
    private (string token, DateTime expiresAt) GenerateJwtToken(Account account)
    {
        var jwt = _config.GetSection("JwtSettings");

        // Mã hóa SecretKey từ file appsettings.json
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tính thời gian hết hạn của token
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpirationInMinutes"]!));

        // Nhét các thông tin cơ bản (ID, Email, Role) vào bên trong Token
        // Giúp Frontend lấy được thông tin user mà không cần gọi thêm API
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim("username",                    account.Username),
            new Claim(ClaimTypes.Role,               account.SystemRole),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        // Đóng gói Token
        var tokenObj = new JwtSecurityToken(issuer: jwt["Issuer"], audience: jwt["Audience"], claims: claims, expires: expiresAt, signingCredentials: creds);

        // Trả về chuỗi Token dạng string
        return (new JwtSecurityTokenHandler().WriteToken(tokenObj), expiresAt);
    }
    // ==========================================
    // 7. TẠO TÀI KHOẢN JUDGE/MENTOR (Dành cho Coordinator)
    // ==========================================
    public async Task CreateAccountByCoordinatorAsync(CreateAccountRequest request, Guid coordinatorId)
    {
        var repo = _uow.GetRepository<Account>();

        // 1. Kiểm tra Role hợp lệ (Chỉ cho phép tạo Judge hoặc Mentor)
        var allowedRoles = new[] { "Judge", "Mentor" };
        if (!allowedRoles.Contains(request.Role))
            throw new BadRequestException("Role không hợp lệ. Chỉ hỗ trợ tạo tài khoản Judge hoặc Mentor.");

        // 2. Kiểm tra Email đã tồn tại chưa
        var existingEmail = await repo.GetFirstOrDefaultAsync(a => a.Email == request.Email);
        if (existingEmail is not null)
            throw new ConflictException("Email này đã được sử dụng trong hệ thống.");

        // 3. Xử lý Username (Dùng request.Username thay vì FullName)
        var normalizedName = RemoveVietnameseTone(request.Username).Replace(" ", "");
        var randomSuffix = Guid.NewGuid().ToString("N")[..4];
        var generatedUsername = $"{normalizedName}_{randomSuffix}";

        // 4. Sinh Mật khẩu ngẫu nhiên (Ví dụ: 8 ký tự alphanumeric)
        var tempPassword = Guid.NewGuid().ToString("N")[..8];

        // 5. Tạo đối tượng Account
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = generatedUsername,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),

            SystemRole = request.Role,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(account);
        await _uow.SaveChangesAsync();

        // 6. Gửi Email thông báo Mật khẩu tạm thời cho họ
        var loginLink = "http://localhost:3000/api/auth/login";
        var emailSubject = $"Thư mời tham gia giải đấu Seal Hackathon 2026 - Vai trò {request.Role}";

        // Đổi lời chào thành request.Username
        var emailBody = $@"
        <h3>Kính gửi {request.Username},</h3>
        <p>Ban tổ chức Seal Hackathon trân trọng kính mời bạn tham gia hệ thống với vai trò <strong>{request.Role}</strong>.</p>
        <p>Tài khoản của bạn đã được khởi tạo thành công. Dưới đây là thông tin đăng nhập của bạn:</p>
        <ul>
            <li><strong>Tên đăng nhập / Email:</strong> {request.Email}</li>
            <li><strong>Mật khẩu tạm thời:</strong> <span style='color: red; font-weight: bold;'>{tempPassword}</span></li>
        </ul>
        <p>Vui lòng đăng nhập vào hệ thống và tiến hành <strong>đổi mật khẩu ngay lần đăng nhập đầu tiên</strong> để đảm bảo tính bảo mật.</p>
        <p><a href='{loginLink}' style='padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>Đăng nhập hệ thống</a></p>
        <p>Trân trọng,<br/>Ban Tổ Chức Seal Hackathon</p>";

        await _emailService.SendEmailAsync(account.Email, emailSubject, emailBody);
    }
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
}