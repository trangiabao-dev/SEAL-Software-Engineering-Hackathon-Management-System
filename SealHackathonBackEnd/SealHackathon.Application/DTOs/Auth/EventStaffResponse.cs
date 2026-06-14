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
        public List<StaffTeamDto> AssignedTeams { get; set; } = new();
        public List<StaffRoundDto> AssignedRounds { get; set; } = new();
    }

    public class StaffTeamDto
    {
        public Guid Id { get; set; }
        public string TeamName { get; set; } = null!;
    }

    public class StaffRoundDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
