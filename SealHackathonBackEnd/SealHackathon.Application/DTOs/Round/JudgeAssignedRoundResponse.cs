using System;

namespace SealHackathon.Application.DTOs.Round
{
    public class JudgeAssignedRoundResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string TrackName { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SubmissionCount { get; set; }
        public string Status { get; set; } = null!;
    }
}
