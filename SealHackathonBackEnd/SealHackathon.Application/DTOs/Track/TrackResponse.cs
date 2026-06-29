namespace SealHackathon.Application.DTOs.Track
{
    // DTO hiển thị thông tin Track
    public class TrackResponse
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxTeams { get; set; }
        public int? MaxMembers { get; set; }
        public int CurrentTeamCount { get; set; }
        public bool IsDeleted { get; set; }
    }
}
