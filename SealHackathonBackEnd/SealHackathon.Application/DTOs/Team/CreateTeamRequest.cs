using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SealHackathon.Application.DTOs.Team
{
    public class CreateTeamRequest
    {
        public string TeamName { get; set; } = null!;
        public string University { get; set; } = null!;
        public int TrackId { get; set; }
        public string? GithubRepoLink { get; set; }
    }
}
