namespace SealHackathon.Application.DTOs.Track
{
    public class AssignMentorTeamsRequest
    {
        public List<Guid> TeamIds { get; set; } = new();
    }
}
