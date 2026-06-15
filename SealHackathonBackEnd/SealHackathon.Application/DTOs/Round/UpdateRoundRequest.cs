namespace SealHackathon.Application.DTOs.Round
{
    // DTO cập nhật Round thông thường
    public class UpdateRoundRequest
    {
        public string Name { get; set; } = null!;
        public int OrderIndex { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? AdvancingSlots { get; set; }
    }
}
