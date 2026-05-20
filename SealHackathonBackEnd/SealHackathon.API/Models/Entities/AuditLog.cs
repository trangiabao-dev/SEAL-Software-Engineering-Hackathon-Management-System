using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SealHackathon.API.Models.Entities;

[Table("AuditLog")]
public partial class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid PerformedBy { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Action { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string EntityName { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string EntityId { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PerformedBy")]
    [InverseProperty("AuditLogs")]
    public virtual Account PerformedByNavigation { get; set; } = null!;
}
