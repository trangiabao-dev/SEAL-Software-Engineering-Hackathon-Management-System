using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Topic")]
public partial class Topic
{
    [Key]
    public int Id { get; set; }

    public int RoundId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? AttachmentUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TopicCreatedByNavigations")]
    public virtual Account? CreatedByNavigation { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("Topics")]
    public virtual Round Round { get; set; } = null!;

    [InverseProperty("Topic")]
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("TopicUpdatedByNavigations")]
    public virtual Account? UpdatedByNavigation { get; set; }
}
