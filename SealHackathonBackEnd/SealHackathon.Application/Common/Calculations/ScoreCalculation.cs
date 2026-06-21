using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Exceptions;

namespace SealHackathon.Application.Common.Calculations
{
    /// <summary>
    /// Cung cấp công thức quy đổi điểm criterion theo MaxScore và Weight.
    /// </summary>
    public static class ScoreCalculation
    {
        // =============== Public methods ===============

        /// <summary>
        /// Quy đổi một điểm criterion sang thang điểm có trọng số.
        /// </summary>
        public static double CalculateWeightedCriterionScore(
            double score,
            double maxScore,
            double weight)
        {
            if (maxScore <= 0)
                throw new BadRequestException(ErrorMessages.Criterion.MaxScoreInvalid);

            return score / maxScore * weight * 100;
        }
    }
}
