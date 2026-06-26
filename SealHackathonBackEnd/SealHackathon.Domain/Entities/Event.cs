using System;
using System.Collections.Generic;

namespace SealHackathon.Domain.Entities;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<EventAccount> EventAccounts { get; set; } = new List<EventAccount>();

    public virtual ICollection<Prize> Prizes { get; set; } = new List<Prize>();

    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();

    public virtual Account? UpdatedByNavigation { get; set; }
}
