using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("JudgeAssign")]
[Index("JudgeId", "RoundId", Name = "UQ_Judge_Round", IsUnique = true)]
public partial class JudgeAssign
{
    [Key]
    public int Id { get; set; }

    public Guid JudgeId { get; set; }

    public int RoundId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    [ForeignKey("AssignedBy")]
    [InverseProperty("JudgeAssignAssignedByNavigations")]
    public virtual Account AssignedByNavigation { get; set; } = null!;

    [ForeignKey("JudgeId")]
    [InverseProperty("JudgeAssignJudges")]
    public virtual Account Judge { get; set; } = null!;

    [ForeignKey("RoundId")]
    [InverseProperty("JudgeAssigns")]
    public virtual Round Round { get; set; } = null!;
}
