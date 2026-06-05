using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.CriterionTemplate
{
    public class CriterionTemplateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public List<CriterionTemplateItemResponse> Items { get; set; } = new List<CriterionTemplateItemResponse>();
    }

    public class CriterionTemplateItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }
    }
}
