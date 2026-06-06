namespace SealHackathon.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string SystemRole { get; set; } = null!; // Bảo thêm cho Thức sửa lại rule Mentor và Judge
        public List<string> Roles { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}