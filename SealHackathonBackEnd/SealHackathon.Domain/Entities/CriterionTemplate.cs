using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class CriterionTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<CriterionTemplateItem> CriterionTemplateItems { get; set; } = new List<CriterionTemplateItem>();
}
