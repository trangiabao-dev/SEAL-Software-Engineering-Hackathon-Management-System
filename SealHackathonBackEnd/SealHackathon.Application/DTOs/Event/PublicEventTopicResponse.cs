using System;

namespace SealHackathon.Application.DTOs.Event
{
    public class PublicEventTopicResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
