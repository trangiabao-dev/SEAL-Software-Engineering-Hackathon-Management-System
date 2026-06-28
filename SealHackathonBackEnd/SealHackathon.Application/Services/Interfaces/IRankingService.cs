using SealHackathon.Application.DTOs.Ranking;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IRankingService
    {
        /// <summary>
        /// Coordinator trigger tính toán ranking cho 1 round — xóa ranking cũ, tính lại từ ScoreRecord, lưu DB
        /// </summary>
        Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId);

        /// <summary>
        /// Lưu thứ tự chính thức của nhóm đồng hạng sau khi Judge xét tiêu chí phụ.
        /// </summary>
        Task<RankingLeaderboardResponse> ResolveTieAsync(
            int roundId,
            ResolveRankingTieRequest request);

        /// <summary>
        /// Lấy bảng xếp hạng đã tính của 1 round — đọc từ DB, không tính lại
        /// </summary>
        Task<RankingLeaderboardResponse> GetLeaderboardByRoundAsync(int roundId);

        /// <summary>
        /// Lấy bảng xếp hạng chính thức của vòng chung kết thuộc một Track.
        /// </summary>
        Task<TrackFinalRankingResponse> GetLeaderboardByTrackAsync(int trackId);

        /// <summary>
        /// Lấy bảng xếp hạng chung cuộc của Event từ Final Round thuộc Track Final.
        /// </summary>
        Task<EventRankingResponse> GetLeaderboardByEventAsync(int eventId);

        /// <summary>
        /// Xuất bảng xếp hạng của một Round đã đóng ra file XLSX.
        /// </summary>
        Task<byte[]> ExportLeaderboardByRoundAsync(int roundId);

        /// <summary>
        /// Xuất bảng xếp hạng chung cuộc của Event ra file XLSX.
        /// </summary>
        Task<byte[]> ExportLeaderboardByEventAsync(int eventId);

        /// <summary>
        /// Lấy ranking của 1 team cụ thể trong 1 round
        /// </summary>
        Task<RankingResponse> GetTeamRankingAsync(int roundId, Guid teamId);
    }
}
