using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Topic
{
    public class UpdateTopicRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
