using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("ScoreRecord")]
[Index("SubmissionId", Name = "IX_ScoreRecord_SubmissionId")]
[Index("SubmissionId", "JudgeId", "CriterionId", Name = "UQ_ScoreRecord", IsUnique = true)]
public partial class ScoreRecord
{
    [Key]
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public Guid JudgeId { get; set; }

    public int CriterionId { get; set; }

    public double Score { get; set; }

    public string? Comment { get; set; }

    public bool? IsCalibration { get; set; }

    public DateTime? ScoredAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CriterionId")]
    [InverseProperty("ScoreRecords")]
    public virtual Criterion Criterion { get; set; } = null!;

    [ForeignKey("JudgeId")]
    [InverseProperty("ScoreRecords")]
    public virtual Account Judge { get; set; } = null!;

    [ForeignKey("SubmissionId")]
    [InverseProperty("ScoreRecords")]
    public virtual Submission Submission { get; set; } = null!;
}
