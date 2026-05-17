namespace SealHackathon.API.Models
{
    // Class này sẽ tương ứng với bảng Teams trong SQL Server
    public class Team
    {
        public int Id { get; set; }

        public string TeamName { get; set; }

        // Mã mời ngẫu nhiên để các sinh viên khác nhập vào và xin gia nhập đội
        public string InviteCode { get; set; }

        // ==========================================
        // KHÓA NGOẠI (FOREIGN KEY)
        // Cột này sẽ lưu Id của sinh viên tạo ra đội này (lấy từ bảng User)
        // ==========================================
        public int LeaderId { get; set; }

        // Lưu lại thời gian tạo đội để sau này Admin dễ quản lý
        public DateTime CreatedAt { get; set; }
    }
}