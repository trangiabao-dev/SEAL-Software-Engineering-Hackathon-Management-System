namespace SealHackathon.Application.DTOs.PublicResults
{
    /// <summary>
    /// Thông tin rút gọn của Event được phép hiển thị cho người xem public.
    /// </summary>
    public class PublicEventSummaryResponse
    {
        /// <summary>
        /// ID của Event, FE dùng để gọi API lấy kết quả chi tiết.
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Tên Event hiển thị trên trang public.
        /// </summary>
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả ngắn của Event.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu Event.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Thời gian kết thúc Event.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái Event. Public chỉ trả Active hoặc Completed.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Cho FE biết Event này đã có kết quả để xem hay chưa.
        /// </summary>
        public bool ResultsAvailable { get; set; }
    }
}
