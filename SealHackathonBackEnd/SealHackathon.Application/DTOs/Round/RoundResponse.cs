namespace SealHackathon.Application.DTOs.Round
{
    // DTO trả về thông tin Round
    public class RoundResponse
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string Name { get; set; } = null!;
        public int OrderIndex { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? AdvancingSlots { get; set; }
        public string Status { get; set; } = null!;
    }
}
