namespace SealHackathon.Application.DTOs.Track
{
    // DTO tạo Track, yêu cầu EventId mà Track này thuộc về
    public class CreateTrackRequest
    {
        public int EventId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxTeams { get; set; } // Hỗ trợ giới hạn số đội, có thể null
    }
}
