namespace SealHackathon.Application.DTOs.Criteria
{
    // DTO dùng để import các tiêu chí từ Template
    public class ImportCriterionRequest
    {
        public int RoundId { get; set; }
        public int TemplateId { get; set; } // ID của CriterionTemplate để import các Items
    }
}
