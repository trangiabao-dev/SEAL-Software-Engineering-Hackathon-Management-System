using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Round")]
public partial class Round
{
    [Key]
    public int Id { get; set; }

    public int TrackId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int OrderIndex { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? AdvancingSlots { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("RoundCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [InverseProperty("Round")]
    public virtual ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();

    [InverseProperty("Round")]
    public virtual ICollection<JudgeAssign> JudgeAssigns { get; set; } = new List<JudgeAssign>();

    [InverseProperty("Round")]
    public virtual ICollection<Ranking> Rankings { get; set; } = new List<Ranking>();

    [InverseProperty("Round")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    [InverseProperty("Round")]
    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    [ForeignKey("TrackId")]
    [InverseProperty("Rounds")]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("RoundUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
