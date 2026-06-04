namespace SealHackathon.Application.DTOs.Topic
{
    // DTO tạo Topic cho một Round
    public class CreateTopicRequest
    {
        public int RoundId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? AttachmentUrl { get; set; } // Link tài liệu đính kèm
    }
}
