namespace SealHackathon.Domain.Constants
{
    public static class NotificationConstants
    {
        public static class Types
        {
            public const string TeamApproved = "TEAM_APPROVED";
            public const string TeamRejected = "TEAM_REJECTED";
            public const string TeamDisqualified = "TEAM_DISQUALIFIED";
            public const string SubmissionDisqualified = "SUBMISSION_DISQUALIFIED";
        }

        public static class Messages
        {
            public const string TeamApprovedTitle = "Đội thi của bạn đã được duyệt!";
            public const string TeamRejectedTitle = "Đội thi của bạn đã bị từ chối!";
            public const string TeamDisqualifiedTitle = "Đội thi của bạn đã bị loại!";
            public const string SubmissionDisqualifiedTitle = "Bài nộp đã bị loại";
        }
    }
}
