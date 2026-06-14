namespace SealHackathon.Application.DTOs.Team
{
    public class TeamGroupedByStatusDto
    {
        public List<TeamListDto> Pending { get; set; } = new();
        public List<TeamListDto> Approved { get; set; } = new();
        public List<TeamListDto> Rejected { get; set; } = new();
        public List<TeamListDto> Disqualified { get; set; } = new();
        public TeamStatusCountDto Counts { get; set; } = new();
    }

    public class TeamStatusCountDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Disqualified { get; set; }
        public int Total { get; set; }
    }
}