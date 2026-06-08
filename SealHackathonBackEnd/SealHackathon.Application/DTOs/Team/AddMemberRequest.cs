using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class AddMemberRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2–100 ký tự.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Mã sinh viên không được để trống.")]
        [RegularExpression(@"^[A-Za-z]{2}\d{6}$", ErrorMessage = "Mã sinh viên phải có dạng 2 chữ cái + 6 chữ số.")]
        public string StudentCode { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Tên trường không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên trường tối đa 200 ký tự.")]
        public string University { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string Phone { get; set; } = null!;

        public bool IsFPTStudent { get; set; }
    }
}
