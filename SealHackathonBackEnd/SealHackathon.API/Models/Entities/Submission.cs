using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Submission")]
[Index("RoundId", Name = "IX_Submission_RoundId")]
[Index("TeamId", "RoundId", Name = "UQ_Submission_Team_Round", IsUnique = true)]
public partial class Submission
{
    [Key]
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public int RoundId { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? DemoUrl { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? ReportUrl { get; set; }

    public string? AiEvaluation { get; set; }

    public bool? IsDisqualified { get; set; }

    [StringLength(500)]
    public string? DisqualifyReason { get; set; }

    public DateTime? DisqualifiedAt { get; set; }

    public Guid? DisqualifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DisqualifiedBy")]
    [InverseProperty("Submissions")]
    public virtual Account? DisqualifiedByNavigation { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("Submissions")]
    public virtual Round Round { get; set; } = null!;

    [InverseProperty("Submission")]
    public virtual ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();

    [ForeignKey("TeamId")]
    [InverseProperty("Submissions")]
    public virtual Team Team { get; set; } = null!;
}
