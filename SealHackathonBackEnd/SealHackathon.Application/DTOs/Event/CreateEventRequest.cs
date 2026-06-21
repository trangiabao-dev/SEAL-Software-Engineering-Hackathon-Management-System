namespace SealHackathon.Application.DTOs.Event
{
    // DTO cho yêu cầu tạo Event mới
    public class CreateEventRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
