using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Prize")]
public partial class Prize
{
    [Key]
    public int Id { get; set; }

    public int TrackId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int RankPosition { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [ForeignKey("TrackId")]
    [InverseProperty("Prizes")]
    public virtual Track Track { get; set; } = null!;
}
