namespace SealHackathon.Application.DTOs.Round
{
    // DTO dùng riêng để cập nhật trạng thái Round
    // Khi Status được set thành Active -> Kích hoạt Rule 7 (Random Topic)
    public class UpdateRoundStatusRequest
    {
        public string Status { get; set; } = null!; // Upcoming, Active, Scoring, Closed
    }
}
