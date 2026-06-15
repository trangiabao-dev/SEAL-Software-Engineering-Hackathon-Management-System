namespace SealHackathon.Application.DTOs.CriterionTemplate
{
    public class CreateCriterionTemplateRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        
        public List<CriterionTemplateItemRequest> Items { get; set; } = new List<CriterionTemplateItemRequest>();
    }

    public class CriterionTemplateItemRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double MaxScore { get; set; }
        public double Weight { get; set; }
    }
}
