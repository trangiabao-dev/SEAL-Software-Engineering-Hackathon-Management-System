namespace SealHackathon.Application.DTOs.Criterion
{
    // DTO tạo tiêu chí chấm điểm
    public class CreateCriterionRequest
    {
        public int RoundId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double MaxScore { get; set; } // Điểm tối đa
        public double Weight { get; set; }   // Trọng số (Tổng các weight trong 1 vòng phải = 1.0)
    }
}
