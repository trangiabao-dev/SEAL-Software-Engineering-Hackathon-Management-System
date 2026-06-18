namespace SealHackathon.Application.DTOs.Prize
{
    /// <summary>
    /// Dữ liệu cập nhật cấu hình giải thưởng.
    /// </summary>
    public class UpdatePrizeRequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int RankPosition { get; set; }

        public decimal? Amount { get; set; }
    }
}
