using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class UpdateTeamRequest
    {
        [Required(ErrorMessage = "Tên đội không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên đội phải từ 2–100 ký tự.")]
        public string TeamName { get; set; } = null!;

        [Required(ErrorMessage = "Tên trường không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên trường tối đa 200 ký tự.")]
        public string University { get; set; } = null!;

        [Url(ErrorMessage = "Link Github không đúng định dạng URL.")]
        [StringLength(500, ErrorMessage = "Link Github tối đa 500 ký tự.")]
        public string? GithubRepoLink { get; set; }
    }
}
