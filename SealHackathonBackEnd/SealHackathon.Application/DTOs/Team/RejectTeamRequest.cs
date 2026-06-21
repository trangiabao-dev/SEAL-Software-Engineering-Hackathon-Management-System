using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class RejectTeamRequest
    {
        [Required(ErrorMessage = "Lý do từ chối đội không được để trống.")]
        [StringLength(500, ErrorMessage = "Lý do từ chối đội tối đa 500 ký tự.")]
        public string Reason { get; set; } = null!;
    }
}
