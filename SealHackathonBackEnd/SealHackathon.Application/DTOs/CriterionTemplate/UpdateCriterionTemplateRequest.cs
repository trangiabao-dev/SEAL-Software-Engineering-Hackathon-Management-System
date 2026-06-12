using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.CriterionTemplate
{
    public class UpdateCriterionTemplateRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public List<CriterionTemplateItemRequest> Items { get; set; } = new();
    }
}