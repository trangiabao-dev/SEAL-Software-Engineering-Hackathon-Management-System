namespace SealHackathon.Domain.Constants
{
    public static class EventAccountConstants
    {
        /// <summary>
        /// Constants dùng riêng cho bảng EventAccount.
        /// EventAccount lưu quyền Mentor/Judge của một account trong một Event cụ thể.
        /// </summary>
        public static class Status
        {
            public const string Approved = "Approved";
            public const string Inactive = "Inactive";
            public const string Rejected = "Rejected";
        }
    }
}