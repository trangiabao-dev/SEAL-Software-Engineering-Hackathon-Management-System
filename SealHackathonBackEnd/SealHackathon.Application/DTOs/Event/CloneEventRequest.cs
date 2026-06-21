using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Event
{
    public class CloneEventRequest
    {
        [Required(ErrorMessage = "Tên giải đấu mới không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên giải đấu không được vượt quá 200 ký tự.")]
        public string NewName { get; set; } = null!;

        [Required(ErrorMessage = "Ngày bắt đầu mới không được để trống.")]
        public DateTime NewStartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc mới không được để trống.")]
        public DateTime NewEndDate { get; set; }
    }
}
