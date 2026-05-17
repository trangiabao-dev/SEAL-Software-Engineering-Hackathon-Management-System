namespace SealHackathon.API.Models
{
    // Class này sẽ tương ứng với bảng Users trong SQL Server
    public class User
    {
        // Khóa chính (Primary Key) của bảng
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        // Cột lưu mật khẩu đã được băm (mã hóa), tuyệt đối không lưu mật khẩu gốc
        public string PasswordHash { get; set; }

        // Vai trò: Student, Judge, Mentor, Admin, Coordinator
        public string Role { get; set; }

        // Trạng thái: PENDING (Chờ duyệt), ACTIVE (Đang hoạt động), REJECTED (Từ chối)
        public string Status { get; set; }

        // Mã số sinh viên (Dành riêng cho sinh viên FPT, các role khác có thể để null nên có dấu ?)
        public string? StudentId { get; set; }
    }
}