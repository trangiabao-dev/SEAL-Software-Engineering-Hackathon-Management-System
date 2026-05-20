using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("CriterionTemplateItem")]
public partial class CriterionTemplateItem
{
    [Key]
    public int Id { get; set; }

    public int TemplateId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double MaxScore { get; set; }

    public double Weight { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("CriterionTemplateItems")]
    public virtual CriterionTemplate Template { get; set; } = null!;
}
