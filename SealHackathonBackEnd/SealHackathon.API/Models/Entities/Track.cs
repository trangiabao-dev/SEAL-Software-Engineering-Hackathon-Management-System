using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Track")]
public partial class Track
{
    [Key]
    public int Id { get; set; }

    public int EventId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? MaxTeams { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TrackCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("Tracks")]
    public virtual Event Event { get; set; } = null!;

    [InverseProperty("Track")]
    public virtual ICollection<MentorAssign> MentorAssigns { get; set; } = new List<MentorAssign>();

    [InverseProperty("Track")]
    public virtual ICollection<Prize> Prizes { get; set; } = new List<Prize>();

    [InverseProperty("Track")]
    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();

    [InverseProperty("Track")]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TrackUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
