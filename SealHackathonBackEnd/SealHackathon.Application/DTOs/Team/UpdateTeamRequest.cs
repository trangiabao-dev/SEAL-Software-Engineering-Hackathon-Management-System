namespace SealHackathon.Application.DTOs.Team
{
    public class UpdateTeamRequest
    {
        public string TeamName { get; set; } = null!;
        public string University { get; set; } = null!;
        public string? GithubRepoLink { get; set; }
    }
}