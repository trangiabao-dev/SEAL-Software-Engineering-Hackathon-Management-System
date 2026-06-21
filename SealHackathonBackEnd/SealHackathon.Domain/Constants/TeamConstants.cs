namespace SealHackathon.Domain.Constants
{
    public static class TeamConstants
    {
        /// <summary>
        /// Constants dùng riêng cho bảng Team.
        /// </summary>
        public static class Status
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Disqualified = "Disqualified";
        }

        /// <summary>
        /// Chuyên quản lý giới hạn team/member
        /// </summary>
        public static class Rules
        {
            public const int MinMembersPerTeam = 3;
            public const int MaxMembersPerTeam = 5;
        }
    }
}