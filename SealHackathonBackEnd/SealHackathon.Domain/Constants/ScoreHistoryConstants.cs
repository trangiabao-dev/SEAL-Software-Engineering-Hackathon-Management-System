using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SealHackathon.Domain.Constants
{
    /// <summary>
    /// Các trạng thái hiển thị trong lịch sử chấm bài của Judge.
    /// </summary>
    public static class ScoreHistoryConstants
    {
        /// <summary>
        /// Giá trị trạng thái trả về cho FE.
        /// </summary>
        public static class Status
        {
            //InProgress: Judge mới chấm một phần criterion
            //Completed:  Judge đã chấm đủ tất cả criterion
            //Locked:  Round đã đóng hoặc ranking đã được tính
            //Disqualified: Submission đã bị loại
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Locked = "Locked";
            public const string Disqualified = "Disqualified";
        }
    }
}
