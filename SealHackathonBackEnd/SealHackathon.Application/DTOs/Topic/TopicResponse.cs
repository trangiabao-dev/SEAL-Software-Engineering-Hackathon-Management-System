namespace SealHackathon.Application.DTOs.Topic
{
    // DTO trả về thông tin Topic
    public class TopicResponse
    {
        public int Id { get; set; }
        public int? RoundId { get; set; }
        public int? EventId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
