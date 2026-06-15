namespace SealHackathon.Application.DTOs.Notification
{
    public class CreateNotificationRequest
    {
        public Guid AccountId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
