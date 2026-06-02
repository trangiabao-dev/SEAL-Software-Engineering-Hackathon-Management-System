namespace SealHackathon.Application.DTOs.Team
{
    public class AddMemberRequest // Leader thêm thành viên mới
    {
        public string FullName { get; set; } = null!;
        public string StudentCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsFPTStudent { get; set; }
    }
}