namespace SealHackathon.Application.DTOs.Team
{
    public class TeamListDto
    {
        public Guid Id { get; set; }
        public string TeamName { get; set; } = null!;
        public string University { get; set; } = null!;
        public int TrackId { get; set; }
        public Guid LeaderId { get; set; }
        public Guid? MentorId { get; set; }
        public string? MentorName { get; set; }
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public string? GithubRepoLink { get; set; }
        public string Status { get; set; } = null!;
        public int MemberCount { get; set; }
        public string? DisqualifyReason { get; set; }
    }
}