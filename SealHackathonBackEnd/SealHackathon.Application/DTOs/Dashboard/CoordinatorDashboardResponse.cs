using System.Collections.Generic;

namespace SealHackathon.Application.DTOs.Dashboard
{
    public class CoordinatorDashboardResponse
    {
        public int TotalActiveTeams { get; set; }
        public int TotalPendingTeams { get; set; }
        public int IncompleteSubmissions { get; set; }
        public List<RoundStatusDto> ActiveRoundStatuses { get; set; } = new List<RoundStatusDto>();
    }

    public class RoundStatusDto
    {
        public string EventName { get; set; } = null!;
        public string TrackName { get; set; } = null!;
        public string RoundName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
