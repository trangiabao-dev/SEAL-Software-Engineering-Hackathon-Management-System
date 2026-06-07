namespace SealHackathon.Application.DTOs.Auth
{
    public class EventStaffResponse
    {
        public Guid AccountId { get; set; }
        public int EventId { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string EventRole { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? JudgeType { get; set; }
    }
}
