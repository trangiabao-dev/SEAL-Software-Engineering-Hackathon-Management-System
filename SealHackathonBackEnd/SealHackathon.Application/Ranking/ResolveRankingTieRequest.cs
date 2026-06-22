namespace SealHackathon.Application.DTOs.Ranking
{
    /// <summary>
    /// Chứa thứ tự chính thức của các đội đồng hạng sau khi Judge xét tiêu chí phụ.
    /// </summary>
    public class ResolveRankingTieRequest
    {
        /// <summary>
        /// Danh sách TeamId theo thứ tự từ hạng cao xuống hạng thấp.
        /// </summary>
        public List<Guid> OrderedTeamIds { get; set; } = new();
    }
}
