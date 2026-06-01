namespace SealHackathon.Application.DTOs.Team
{
    public class CreateTeamRequest
    {
        // Thông tin Team
        public string TeamName { get; set; } = null!;
        public string University { get; set; } = null!;
        public int TrackId { get; set; }
        public string? GithubRepoLink { get; set; }

        // Thông tin Leader — để lưu vào TeamMember
        public string FullName { get; set; } = null!;
        public string StudentCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsFPTStudent { get; set; }
    }
}
