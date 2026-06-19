using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SealHackathon.Application.DTOs.Event
{
    public class CreateFullEventRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }

        public List<CreateFullEventTrackDto> Tracks { get; set; } = new();
    }

    public class CreateFullEventTrackDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        public int? MaxTeams { get; set; }

        public List<CreateFullEventRoundDto> Rounds { get; set; } = new();
    }

    public class CreateFullEventRoundDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        public int? AdvancingSlots { get; set; }
        
        public List<CreateFullEventTopicDto> Topics { get; set; } = new();
    }

    public class CreateFullEventTopicDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        public string? Requirements { get; set; }
        
        public string? AttachmentUrl { get; set; }
    }
}
