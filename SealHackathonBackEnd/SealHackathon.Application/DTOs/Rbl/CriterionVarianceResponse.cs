using System;

namespace SealHackathon.Application.DTOs.Rbl
{
    public class CriterionVarianceResponse
    {
        public int CriterionId { get; set; }
        public string CriterionName { get; set; } = string.Empty;
        public string RoundName { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public double Variance { get; set; }
        public int SubmissionsCount { get; set; }
    }
}
