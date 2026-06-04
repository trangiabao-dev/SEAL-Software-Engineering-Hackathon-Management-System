using SealHackathon.Application.Ranking;

namespace SealHackathon.Application.Services.Interfaces
{
    public interface IRankingService
    {
        /// <summary>
        /// Coordinator trigger: Tính toán ranking cho 1 round.
        /// Xóa ranking cũ (nếu có) → tính lại từ ScoreRecord → lưu DB.
        /// </summary>
        /// <param name="roundId">ID vòng thi cần tính ranking</param>
        /// <returns>Bảng xếp hạng mới</returns>
        Task<RankingLeaderboardResponse> CalculateRankingAsync(int roundId);

        /// <summary>
        /// Lấy bảng xếp hạng đã tính của 1 round (đọc từ DB).
        /// </summary>
        /// <param name="roundId">ID vòng thi</param>
        /// <returns>Bảng xếp hạng</returns>
        Task<RankingLeaderboardResponse> GetLeaderboardByRoundAsync(int roundId);

        /// <summary>
        /// Lấy ranking của 1 team cụ thể trong 1 round.
        /// </summary>
        /// <param name="roundId">ID vòng thi</param>
        /// <param name="teamId">ID đội</param>
        /// <returns>Ranking của đội</returns>
        Task<RankingResponse> GetTeamRankingAsync(int roundId, Guid teamId);
    }
}                   
