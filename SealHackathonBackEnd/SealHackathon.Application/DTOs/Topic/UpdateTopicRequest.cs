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
        
        [Url(ErrorMessage = "Link đính kèm phải là URL hợp lệ.")]
        [RegularExpression(@"^https://.*", ErrorMessage = "Link đính kèm phải bắt đầu bằng https://")]
        [MaxLength(1000, ErrorMessage = "Link đính kèm không được vượt quá 1000 ký tự.")]
        public string? AttachmentUrl { get; set; }
    }
}
