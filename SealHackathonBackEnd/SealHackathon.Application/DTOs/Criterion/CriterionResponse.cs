namespace SealHackathon.Application.DTOs.Criterion
{
    // DTO trả về thông tin tiêu chí chấm điểm
    public class CriterionResponse
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }
    }
}
