using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Submission
{
    public class UpdateSubmissionRequest
    {
        [Url(ErrorMessage = "DemoUrl không đúng định dạng Url.")]
        [StringLength(1000, ErrorMessage = "DemoUrl không được vượt quá 1000 ký tự.")]
        public string? DemoUrl { get; set; }

        [Url(ErrorMessage = "ReportUrl không đúng định dạng Url.")]
        [StringLength(1000, ErrorMessage = "ReportUrl tối đa 1000 ký tự.")]
        public string? ReportUrl { get; set; }
    }
}
