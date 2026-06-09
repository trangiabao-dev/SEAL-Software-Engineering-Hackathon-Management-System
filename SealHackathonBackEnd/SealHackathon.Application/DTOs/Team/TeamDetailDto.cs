using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Team
{
    public class TeamDetailDto
    {
        public Guid Id { get; set; }
        public string TeamName { get; set; } = null!;
        public string University { get; set; } = null!;
        public int TrackId { get; set; }
        public Guid LeaderId { get; set; }
        public Guid? MentorId { get; set; }
        public int? TopicId { get; set; }
        public string? GithubRepoLink { get; set; }
        public string Status { get; set; } = null!;
        public List<TeamMemberDto> Members { get; set; } = new();
        public string? DisqualifyReason { get; set; }
    }
}