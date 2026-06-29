using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Event
{
    public class FullEventResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public List<FullTrackResponse> Tracks { get; set; } = new();
    }

    public class FullTrackResponse
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxTeams { get; set; }
        public int? MaxMembers { get; set; }

        public int CurrentTeamCount { get; set; }

        public bool IsFinal { get; set; }


        public List<FullRoundResponse> Rounds { get; set; } = new();
    }

    public class FullRoundResponse
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string Name { get; set; } = null!;
        public int OrderIndex { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? AdvancingSlots { get; set; }
        public string Status { get; set; } = null!;
        public List<FullTopicResponse> Topics { get; set; } = new();
    }

    public class FullTopicResponse
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
