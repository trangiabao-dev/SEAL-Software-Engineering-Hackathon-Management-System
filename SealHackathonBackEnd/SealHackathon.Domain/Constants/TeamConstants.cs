namespace SealHackathon.Domain.Constants
{
    public static class TeamConstants
    {
        // Nhóm 1: Chuyên quản lý trạng thái
        public static class Status
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Disqualified = "Disqualified";

            /// <summary>
            /// Hộp chứa các status của TeamConstants
            /// </summary>
            public static readonly HashSet<string> ValidStatuses =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Pending,
                    Approved,
                    Rejected,
                    Disqualified
                };
        }

        // Nhóm 2: Chuyên quản lý giới hạn nghiệp vụ
        public static class Rules
        {
            public const int MinMembersPerTeam = 3;
            public const int MaxMembersPerTeam = 5;
            public const int MaxTeamsPerMentor = 3;
        }
    }
}