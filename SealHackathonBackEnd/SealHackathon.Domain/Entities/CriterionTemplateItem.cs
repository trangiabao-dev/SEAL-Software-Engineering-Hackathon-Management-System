using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class CriterionTemplateItem
{
    public int Id { get; set; }

    public int TemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double MaxScore { get; set; }

    public double Weight { get; set; }

    public virtual CriterionTemplate Template { get; set; } = null!;
}
