using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("Ranking")]
[Index("TeamId", "RoundId", Name = "UQ_Ranking_Team_Round", IsUnique = true)]
public partial class Ranking
{
    [Key]
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public int RoundId { get; set; }

    public double TotalScore { get; set; }

    public int RankPosition { get; set; }

    public bool? IsAdvancing { get; set; }

    public DateTime? CalculatedAt { get; set; }

    [ForeignKey("RoundId")]
    [InverseProperty("Rankings")]
    public virtual Round Round { get; set; } = null!;

    [ForeignKey("TeamId")]
    [InverseProperty("Rankings")]
    public virtual Team Team { get; set; } = null!;
}
