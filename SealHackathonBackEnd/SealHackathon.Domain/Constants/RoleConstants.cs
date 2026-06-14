namespace SealHackathon.Domain.Constants
{
    public static class RoleConstants
    {
        public const string Pending = "Pending";
        public const string Coordinator = "Coordinator";
        public const string Leader = "Leader";

        // Dùng cho tài khoản khách mời chưa có event role active.
        public const string Inactive = "Inactive";

        // Chúng dùng trong EventAccount.EventRole và JWT claim khi event đang active.
        public const string Mentor = "Mentor";
        public const string Judge = "Judge";
    }
}