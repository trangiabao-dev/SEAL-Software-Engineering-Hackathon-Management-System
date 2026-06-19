namespace SealHackathon.Application.DTOs.Prize
{
    /// <summary>
    /// Dữ liệu tạo cấu hình giải thưởng cho một Track.
    /// </summary>
    public class CreatePrizeRequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int RankPosition { get; set; }

        public decimal? Amount { get; set; }
    }
}
