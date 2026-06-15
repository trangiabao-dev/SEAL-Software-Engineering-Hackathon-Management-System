namespace SealHackathon.Application.DTOs.Event
{
    // DTO cho yêu cầu cập nhật Event hiện có
    public class UpdateEventRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!; // Draft, Active, Closed
    }
}
