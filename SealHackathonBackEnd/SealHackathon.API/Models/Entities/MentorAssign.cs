using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("MentorAssign")]
[Index("MentorId", "TrackId", Name = "UQ_Mentor_Track", IsUnique = true)]
public partial class MentorAssign
{
    [Key]
    public int Id { get; set; }

    public Guid MentorId { get; set; }

    public int TrackId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public Guid AssignedBy { get; set; }

    [ForeignKey("AssignedBy")]
    [InverseProperty("MentorAssignAssignedByNavigations")]
    public virtual Account AssignedByNavigation { get; set; } = null!;

    [ForeignKey("MentorId")]
    [InverseProperty("MentorAssignMentors")]
    public virtual Account Mentor { get; set; } = null!;

    [ForeignKey("TrackId")]
    [InverseProperty("MentorAssigns")]
    public virtual Track Track { get; set; } = null!;
}
