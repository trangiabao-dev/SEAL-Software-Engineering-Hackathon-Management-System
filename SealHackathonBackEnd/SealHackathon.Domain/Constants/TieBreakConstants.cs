namespace SealHackathon.Domain.Constants
{
    /// <summary>
    /// Các hằng số trạng thái dùng cho phiên chấm lại tie-break.
    /// </summary>
    public static class TieBreakConstants
    {
        /// <summary>
        /// Trạng thái vòng đời của một phiên tie-break.
        /// </summary>
        public static class Status
        {
            public const string PendingScoring = "PendingScoring";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
        }

        /// <summary>
        /// Loại thông báo liên quan đến tie-break.
        /// </summary>
        public static class NotificationType
        {
            public const string Required = "TIE_BREAK_REQUIRED";
        }
    }
}
