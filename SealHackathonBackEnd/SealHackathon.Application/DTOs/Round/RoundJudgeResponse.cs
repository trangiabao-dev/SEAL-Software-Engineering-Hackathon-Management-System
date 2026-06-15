using System;

namespace SealHackathon.Application.DTOs.Round
{
    public class RoundJudgeResponse
    {
        public Guid JudgeId { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? JudgeType { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
