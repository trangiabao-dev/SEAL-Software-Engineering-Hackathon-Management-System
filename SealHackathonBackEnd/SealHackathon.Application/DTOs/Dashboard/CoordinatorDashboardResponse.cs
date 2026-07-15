namespace SealHackathon.Application.DTOs.Dashboard
{
    public class CoordinatorDashboardResponse
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public DashboardHighlightDto Highlight { get; set; } = new();
        public List<DashboardEventDto> Events { get; set; } = new();
        public DashboardChartsDto Charts { get; set; } = new();
    }

    public class DashboardSummaryDto
    {
        public int TotalEvents { get; set; }
        public int TotalTeams { get; set; }
        public int TotalSubmissions { get; set; }
        public int IncompleteSubmissions { get; set; }
        public int TotalMentors { get; set; }
        public int TotalJudges { get; set; }
        public Dictionary<string, int> EventsByStatus { get; set; } = new();
        public Dictionary<string, int> RoundsByStatus { get; set; } = new();
        public Dictionary<string, int> TeamsByStatus { get; set; } = new();
    }

    public class DashboardHighlightDto
    {
        public DashboardHighlightEventDto? EventWithMostTeams { get; set; }
    }

    public class DashboardHighlightEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalTeams { get; set; }
    }

    public class DashboardEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int TotalTracks { get; set; }
        public int TotalRounds { get; set; }

        public int TotalTeams { get; set; }
        public int ApprovedTeams { get; set; }
        public int PendingTeams { get; set; }
        public int RejectedTeams { get; set; }
        public int DisqualifiedTeams { get; set; }

        public int TotalSubmissions { get; set; }
        public int IncompleteSubmissions { get; set; }

        public int ActiveRounds { get; set; }
        public int ScoringRounds { get; set; }
        public int ClosedRounds { get; set; }
        public bool ResultsAvailable { get; set; }
    }

    public class DashboardChartsDto
    {
        public List<ChartTeamByEventDto> TeamCountByEvent { get; set; } = new();
        public List<ChartTeamByTrackDto> TeamCountByTrack { get; set; } = new();
        public List<ChartSubmissionByEventDto> SubmissionCountByEvent { get; set; } = new();
    }

    public class ChartTeamByEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalTeams { get; set; }
    }

    public class ChartTeamByTrackDto
    {
        public int EventId { get; set; }
        public int TrackId { get; set; }
        public string TrackName { get; set; } = string.Empty;
        public int TotalTeams { get; set; }
    }

    public class ChartSubmissionByEventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int TotalSubmissions { get; set; }
    }
}
