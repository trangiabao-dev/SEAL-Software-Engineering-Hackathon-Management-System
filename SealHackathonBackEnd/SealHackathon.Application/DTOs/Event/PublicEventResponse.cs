using System;

namespace SealHackathon.Application.DTOs.Event
{
    public class PublicEventResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public string? Location { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
        public int TrackCount { get; set; }
        public bool ResultsAvailable { get; set; }
    }
}
