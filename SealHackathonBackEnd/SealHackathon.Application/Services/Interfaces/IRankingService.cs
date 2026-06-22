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
        /// Lấy ranking của 1 team cụ thể trong 1 round
        /// </summary>
        Task<RankingResponse> GetTeamRankingAsync(int roundId, Guid teamId);
    }
}
