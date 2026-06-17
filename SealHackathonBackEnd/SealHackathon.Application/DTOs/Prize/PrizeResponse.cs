namespace SealHackathon.Application.DTOs.Prize
{
    /// <summary>
    /// Dữ liệu cấu hình giải thưởng trả về cho client.
    /// </summary>
    public class PrizeResponse
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int RankPosition { get; set; }

        public decimal? Amount { get; set; }
    }
}
