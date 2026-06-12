namespace SealHackathon.Application.DTOs.Criteria
{
    // DTO tạo tiêu chí chấm điểm
    public class CreateCriterionRequest
    {
        public int RoundId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }   // FE gửi 30 = 30%. DB lưu 0.30.
    }
}
