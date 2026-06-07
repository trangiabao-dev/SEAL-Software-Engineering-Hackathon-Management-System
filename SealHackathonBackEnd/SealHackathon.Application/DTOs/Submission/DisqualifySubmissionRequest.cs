using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Submission
{
    public class DisqualifySubmissionRequest
    {
        [Required(ErrorMessage = "Lý do loại bài nộp không được để trống.")]
        [StringLength(1000, ErrorMessage = "Lý do loại bài nộp tối đa 1000 ký tự.")]
        public string Reason { get; set; } = null!;

    }
}
