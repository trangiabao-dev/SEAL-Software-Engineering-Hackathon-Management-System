namespace SealHackathon.Application.DTOs.Auth
{
    public class AssignEventRoleRequest
    {
        public Guid AccountId { get; set; }
        public string EventRole { get; set; } = null!;
        public string? JudgeType { get; set; }
    }
}