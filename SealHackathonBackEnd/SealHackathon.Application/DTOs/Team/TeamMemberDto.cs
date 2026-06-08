namespace SealHackathon.Application.DTOs.Team
{
    public class TeamMemberDto // Trả về thông tin thành viên
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string StudentCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string University { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsLeader { get; set; }
        public bool IsFPTStudent { get; set; }
    }
}