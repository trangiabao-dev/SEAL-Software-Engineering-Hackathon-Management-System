using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Submission
{
    public class DisqualifySubmissionRequest
    {
        [Required(ErrorMessage = "Lý do loại bài nộp không được để trống.")]
        [StringLength(500, ErrorMessage = "Lý do loại bài nộp tối đa 500 ký tự.")]
        public string Reason { get; set; } = null!;

    }
}
