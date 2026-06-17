using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Submission
{
    public class CreateSubmissionRequest
    {
        [Required(ErrorMessage = "Link bài thuyết trình không được để trống.")]
        [Url(ErrorMessage = "Link bài thuyết trình không đúng định dạng URL.")]
        [StringLength(1000, ErrorMessage = "Link bài thuyết trình không được vượt quá 1000 ký tự.")]
        public string PresentationUrl { get; set; } = null!;
    }
}