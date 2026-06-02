namespace SealHackathon.Application.DTOs.Team
{
    public class UpdateMemberRequest // Leader sửa thông tin thành viên
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsFPTStudent { get; set; }
    }
}