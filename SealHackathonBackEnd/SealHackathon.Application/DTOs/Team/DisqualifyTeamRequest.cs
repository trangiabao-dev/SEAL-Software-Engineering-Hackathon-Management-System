using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class DisqualifyTeamRequest
    {
        [Required(ErrorMessage = "Lý do loại đội không được để trống.")]
        [StringLength(500, ErrorMessage = "Lý do loại đội tối đa 500 ký tự.")]
        public string Reason { get; set; } = null!;
    }
}