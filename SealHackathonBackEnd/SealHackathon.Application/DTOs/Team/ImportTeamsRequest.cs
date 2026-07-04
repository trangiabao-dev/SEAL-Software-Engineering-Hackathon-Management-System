using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class ImportTeamsRequest
    {
        public string DefaultPassword { get; set; } = "12345";
        public string DefaultStatus { get; set; } = "Approved";
        public List<ImportTeamDto> Teams { get; set; } = new List<ImportTeamDto>();
    }

    public class ImportTeamDto
    {
        public int RowNumber { get; set; }
        public int TrackId { get; set; }
        [Required(ErrorMessage = "Tên team không được để trống")]
        public string? TeamName { get; set; }
        public string? University { get; set; }
        public string? GithubRepoLink { get; set; }
        public ImportMemberDto? Leader { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? University { get; set; }
        public string? GithubRepoLink { get; set; }
        public ImportMemberDto Leader { get; set; } = new ImportMemberDto();
        public List<ImportMemberDto> Members { get; set; } = new List<ImportMemberDto>();
    }

    public class ImportMemberDto
    {
        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        public string StudentCode { get; set; } = string.Empty;
        
        public string? Phone { get; set; }
        public string? University { get; set; }
        public bool IsFPTStudent { get; set; }
        public string? Password { get; set; }
    }

    public class ImportTeamSuccessDto
    {
        public int RowNumber { get; set; }
        public Guid TeamId { get; set; }
        public string? TeamName { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TrackId { get; set; }
        public Guid LeaderId { get; set; }
    }
}
