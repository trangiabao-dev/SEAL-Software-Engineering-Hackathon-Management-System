namespace SealHackathon.Application.DTOs.Track
{
    // DTO cập nhật Track
    public class UpdateTrackRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxTeams { get; set; }
        public int? MaxMembers { get; set; }
        public bool IsFinal { get; set; }
    }
}
