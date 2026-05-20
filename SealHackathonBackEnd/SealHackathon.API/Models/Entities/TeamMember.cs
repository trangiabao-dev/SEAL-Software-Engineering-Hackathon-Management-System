using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("TeamMember")]
public partial class TeamMember
{
    [Key]
    public int Id { get; set; }

    public Guid TeamId { get; set; }

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string StudentCode { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    public bool? IsLeader { get; set; }

    [Column("IsFPTStudent")]
    public bool? IsFptstudent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TeamMemberCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("TeamId")]
    [InverseProperty("TeamMembers")]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TeamMemberUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
