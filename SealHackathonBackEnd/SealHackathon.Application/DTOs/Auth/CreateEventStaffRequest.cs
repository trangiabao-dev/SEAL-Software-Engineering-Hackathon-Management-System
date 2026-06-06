namespace SealHackathon.Application.DTOs.Auth
{
    public class CreateEventStaffRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string EventRole { get; set; } = null!;
        public string? JudgeType { get; set; }
    }
}