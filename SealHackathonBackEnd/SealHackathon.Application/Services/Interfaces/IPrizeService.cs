using SealHackathon.Application.DTOs.Prize;

namespace SealHackathon.Application.Services.Interfaces
{
    /// <summary>
    /// Cung cấp nghiệp vụ quản lý cấu hình giải thưởng và xuất kết quả đạt giải.
    /// </summary>
    public interface IPrizeService
    {
        /// <summary>
        /// Lấy danh sách cấu hình giải thưởng của một Track.
        /// </summary>
        Task<List<PrizeResponse>> GetPrizesByTrackAsync(int trackId);

        /// <summary>
        /// Tạo cấu hình giải thưởng cho một Track.
        /// </summary>
        Task<PrizeResponse> CreatePrizeAsync(int trackId, CreatePrizeRequest request);

        /// <summary>
        /// Cập nhật cấu hình giải thưởng.
        /// </summary>
        Task<PrizeResponse> UpdatePrizeAsync(int prizeId, UpdatePrizeRequest request);

        /// <summary>
        /// Xóa cấu hình giải thưởng.
        /// </summary>
        Task<bool> DeletePrizeAsync(int prizeId);

        /// <summary>
        /// Lấy danh sách đội đạt giải hạng 1, 2, 3 của một Round.
        /// </summary>
        Task<List<PrizeWinnerResponse>> GetWinnersByRoundAsync(int roundId);

        /// <summary>
        /// Xuất danh sách đội đạt giải của một Round ra file XLSX.
        /// </summary>
        Task<byte[]> ExportWinnersByRoundAsync(int roundId);
    }
}
