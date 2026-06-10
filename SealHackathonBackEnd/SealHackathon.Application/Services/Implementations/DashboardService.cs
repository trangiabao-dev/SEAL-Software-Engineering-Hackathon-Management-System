using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Dashboard;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
using System.Linq;
using System.Threading.Tasks;
using SealHackathon.Domain.Constants;

namespace SealHackathon.Application.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<CoordinatorDashboardResponse>> GetCoordinatorDashboardAsync()
        {
            var eventRepo = _uow.GetRepository<Event>();
            var trackRepo = _uow.GetRepository<Track>();
            var teamRepo = _uow.GetRepository<Team>();
            var roundRepo = _uow.GetRepository<Round>();
            var submissionRepo = _uow.GetRepository<Submission>();
            var criterionRepo = _uow.GetRepository<Criterion>();
            var judgeAssignRepo = _uow.GetRepository<JudgeAssign>();
            var scoreRecordRepo = _uow.GetRepository<ScoreRecord>();

            // 1. Get Active Events
            var events = await eventRepo.GetAllAsync(e => !e.IsDeleted);
            var activeEvents = events.Where(e => 
                e.Status.Equals(EventConstants.Status.Active, System.StringComparison.OrdinalIgnoreCase) || 
                e.Status.Equals("Ongoing", System.StringComparison.OrdinalIgnoreCase) || 
                e.Status.Equals("Upcoming", System.StringComparison.OrdinalIgnoreCase)
            ).ToList();
            var activeEventIds = activeEvents.Select(e => e.Id).ToList();

            // 2. Get Tracks for active events
            var tracks = await trackRepo.GetAllAsync(t => !t.IsDeleted);
            var activeTracks = tracks.Where(t => activeEventIds.Contains(t.EventId)).ToList();
            var activeTrackIds = activeTracks.Select(t => t.Id).ToList();

            // 3. Get Teams
            var teams = await teamRepo.GetAllAsync(t => !t.IsDeleted);
            var activeTeams = teams.Where(t => activeTrackIds.Contains(t.TrackId)).ToList();

            int totalActiveTeams = activeTeams.Count;
            int totalPendingTeams = activeTeams.Count(t => t.Status == "Pending"); // Assuming "Pending" is the string

            // 4. Get Rounds & Statuses
            var rounds = await roundRepo.GetAllAsync(r => true);
            var activeRounds = rounds.Where(r => activeTrackIds.Contains(r.TrackId)).ToList();
            var activeRoundIds = activeRounds.Select(r => r.Id).ToList();

            var activeRoundStatuses = activeRounds.Select(r =>
            {
                var track = activeTracks.FirstOrDefault(t => t.Id == r.TrackId);
                var ev = activeEvents.FirstOrDefault(e => e.Id == track?.EventId);
                return new RoundStatusDto
                {
                    EventName = ev?.Name ?? "Unknown Event",
                    TrackName = track?.Name ?? "Unknown Track",
                    RoundName = r.Name,
                    Status = r.Status
                };
            }).ToList();

            // 5. Incomplete Submissions
            var submissions = await submissionRepo.GetAllAsync(s => true);
            var activeSubmissions = submissions.Where(s => activeRoundIds.Contains(s.RoundId)).ToList();

            var criteria = await criterionRepo.GetAllAsync(c => true);
            var judgeAssigns = await judgeAssignRepo.GetAllAsync(j => true);
            var scoreRecords = await scoreRecordRepo.GetAllAsync(s => true);

            int incompleteSubmissionsCount = 0;

            foreach (var sub in activeSubmissions)
            {
                var criteriaCount = criteria.Count(c => c.RoundId == sub.RoundId);
                var judgeCount = judgeAssigns.Count(j => j.RoundId == sub.RoundId);
                var expectedScores = criteriaCount * judgeCount;

                var actualScores = scoreRecords.Count(s => s.SubmissionId == sub.Id);

                if (expectedScores > 0 && actualScores < expectedScores)
                {
                    incompleteSubmissionsCount++;
                }
            }

            var response = new CoordinatorDashboardResponse
            {
                TotalActiveTeams = totalActiveTeams,
                TotalPendingTeams = totalPendingTeams,
                ActiveRoundStatuses = activeRoundStatuses,
                IncompleteSubmissions = incompleteSubmissionsCount
            };

            return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(response, "Lấy dữ liệu Dashboard thành công.");
        }
    }
}
