using SealHackathon.Application.DTOs.Topic;

namespace SealHackathon.Application.DTOs.Team
{
    public class MyActiveRoundResponse
    {
        public int RoundId { get; set; }
        public string RoundName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool CanSubmit { get; set; }
        public TopicResponse? Topic { get; set; }
    }
}