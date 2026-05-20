using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Event")]
public partial class Event
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("EventCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [InverseProperty("Event")]
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("EventUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
