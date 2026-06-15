namespace SealHackathon.Application.DTOs.Track
{
    public class MentorTeamAssignmentResponse
    {
        public int TrackId { get; set; }
        public Guid MentorId { get; set; }
        public List<MentorAssignedTeamDto> AssignedTeams { get; set; } = new();
    }

    public class MentorAssignedTeamDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
    }
}
