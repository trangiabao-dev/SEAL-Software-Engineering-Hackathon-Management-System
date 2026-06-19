using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Dashboard
{
    public class MentorDashboardResponse
    {
        public List<MentorDashboardTeamDto> Teams { get; set; } = new();
    }

    public class MentorDashboardTeamDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string TrackName { get; set; } = null!;
        public string? ActiveRoundName { get; set; }
        public string? ActiveRoundStatus { get; set; }
        public string? TopicName { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
