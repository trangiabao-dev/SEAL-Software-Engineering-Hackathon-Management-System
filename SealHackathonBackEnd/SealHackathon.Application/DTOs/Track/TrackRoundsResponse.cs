using System;
using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Track
{
    public class TrackRoundsResponse
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public List<RoundTimelineDto> Rounds { get; set; } = new List<RoundTimelineDto>();
    }

    public class RoundTimelineDto
    {
        public int RoundId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = null!;
        public int? AdvancingSlots { get; set; }
        public int ProgressPercentage { get; set; } // % thời gian đã trôi qua hoặc tiến độ chấm điểm
    }
}
