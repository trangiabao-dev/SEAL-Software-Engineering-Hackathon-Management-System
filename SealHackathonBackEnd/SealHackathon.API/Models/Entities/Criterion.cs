using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Criterion")]
public partial class Criterion
{
    [Key]
    public int Id { get; set; }

    public int RoundId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double MaxScore { get; set; }

    public double Weight { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("CriterionCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("Criteria")]
    public virtual Round Round { get; set; } = null!;

    [InverseProperty("Criterion")]
    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("CriterionUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
