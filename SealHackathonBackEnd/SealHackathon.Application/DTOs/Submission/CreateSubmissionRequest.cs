using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Submission
{
    public class CreateSubmissionRequest
    {
        [Url(ErrorMessage = "DemoUrl không đúng định dạng URL.")]
        [StringLength(1000, ErrorMessage = "DemoUrl không được vượt quá 1000 ký tự.")]
        public string? DemoUrl { get; set; }

        [Url(ErrorMessage = "ReportUrl không đúng định dạng URL.")]
        [StringLength (1000, ErrorMessage = "ReportUrl không được vượt quá 1000 ký tự.")]
        public string? ReportUrl { get; set; }
    }
}
