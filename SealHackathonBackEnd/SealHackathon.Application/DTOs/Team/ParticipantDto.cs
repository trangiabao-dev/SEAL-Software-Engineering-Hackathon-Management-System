using System;

namespace SealHackathon.Application.DTOs.Team
{
    public class ParticipantDto
    {
        public int Id { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string StudentCode { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string University { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsLeader { get; set; }
        public bool? IsFPTStudent { get; set; }
    }
}
