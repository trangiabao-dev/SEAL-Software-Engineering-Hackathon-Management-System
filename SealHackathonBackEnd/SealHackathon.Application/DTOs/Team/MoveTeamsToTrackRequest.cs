using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Team
{
    public class MoveTeamsToTrackRequest
    {
        [Required]
        public List<Guid> TeamIds { get; set; } = new List<Guid>();
    }
}
