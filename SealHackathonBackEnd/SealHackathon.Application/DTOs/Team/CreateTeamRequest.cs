using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class CreateTeamRequest
    {
        // Thông tin Team
        [Required(ErrorMessage = "Tên đội không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên đội phải từ 2–100 ký tự.")]
        public string TeamName { get; set; } = null!;

        [Required(ErrorMessage = "Tên trường không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên trường tối đa 200 ký tự.")]
        public string University { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "TrackId phải lớn hơn 0")]
        public int TrackId { get; set; }

        [Url(ErrorMessage = "Link Github không đúng định dạng URL.")]
        [StringLength(500, ErrorMessage = "Link Github tối đa 500 ký tự.")]
        public string? GithubRepoLink { get; set; }

        // Thông tin Leader — để lưu vào TeamMember
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2–100 ký tự.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Mã sinh viên không được để trống.")]
        [RegularExpression(@"^[A-Za-z]{2}\d{6}$", ErrorMessage = "Mã sinh viên phải có dạng 2 chữ cái + 6 chữ số (VD: SE170001).")]
        public string StudentCode { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(256, ErrorMessage = "Email tối đa 256 ký tự.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự.")]
        public string Phone { get; set; } = null!;

        public bool IsFPTStudent { get; set; }

        [Required(ErrorMessage = "Danh sách thành viên không được để trống.")]
        [MinLength(2, ErrorMessage = "Team phải có ít nhất 3 người, bao gồm Leader.")]
        [MaxLength(4, ErrorMessage = "Team chỉ được có tối đa 5 người, bao gồm Leader.")]
        public List<AddMemberRequest> Members { get; set; } = new();
    }
}
