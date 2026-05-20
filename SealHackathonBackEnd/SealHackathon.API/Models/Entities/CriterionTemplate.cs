using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("CriterionTemplate")]
public partial class CriterionTemplate
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("CriterionTemplates")]
    public virtual Account? CreatedByNavigation { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<CriterionTemplateItem> CriterionTemplateItems { get; set; } = new List<CriterionTemplateItem>();
}
