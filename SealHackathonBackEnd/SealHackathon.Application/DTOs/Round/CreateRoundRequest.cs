using System;

namespace SealHackathon.Application.DTOs.Round
{
    // DTO tạo mới Round trong một Track cụ thể
    public class CreateRoundRequest
    {
        public int TrackId { get; set; }
        public string Name { get; set; } = null!;
        public int OrderIndex { get; set; } // Thứ tự của vòng thi
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? AdvancingSlots { get; set; } // Số lượng đội được đi tiếp vào vòng sau
    }
}
