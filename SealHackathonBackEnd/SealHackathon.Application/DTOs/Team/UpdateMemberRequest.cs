using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class UpdateMemberRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2–100 ký tự.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string? Phone { get; set; }

        public bool IsFPTStudent { get; set; }
    }
}
