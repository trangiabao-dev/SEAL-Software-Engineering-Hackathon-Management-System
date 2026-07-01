using SealHackathon.Application.DTOs.PublicResults;

namespace SealHackathon.Application.Services.Interfaces
{
    /// <summary>
    /// Cung cấp dữ liệu kết quả công khai cho trang Results không cần đăng nhập.
    /// </summary>
    public interface IPublicResultsService
    {
        /// <summary>
        /// Lấy danh sách Event public đang Active hoặc đã Completed.
        /// </summary>
        Task<List<PublicEventSummaryResponse>> GetPublicEventsAsync();

        /// <summary>
        /// Lấy kết quả công khai của một Event sau khi Event đã đủ điều kiện công bố.
        /// </summary>
        Task<PublicEventResultsResponse> GetEventResultsAsync(int eventId);
    }
}
