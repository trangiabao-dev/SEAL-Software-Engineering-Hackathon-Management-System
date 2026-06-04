using System;

namespace SealHackathon.Application.DTOs.Event
{
    // DTO trả về thông tin chi tiết của một Event
    public class EventResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public bool IsDeleted { get; set; }
    }
}
