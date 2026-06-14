using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Dashboard;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
using System.Linq;
using System.Threading.Tasks;

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

            var activeEvents = await eventRepo.GetAllAsync
                (e => !e.IsDeleted &&
                (e.Status == EventConstants.Status.Active ||
                e.Status == EventConstants.Status.Registration));

            if (!activeEvents.Any())
            {
                return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                    new CoordinatorDashboardResponse(), "Lấy dữ liệu dashboard thành công.");
            }

            var activeEventIds = activeEvents.Select(e => e.Id).ToList();
            var eventById = activeEvents.ToDictionary(e => e.Id);

            var activeTracks = await trackRepo.GetAllAsync(
                t => !t.IsDeleted && activeEventIds.Contains(t.EventId));

            if (!activeTracks.Any())
            {
                return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                    new CoordinatorDashboardResponse(), "Lấy dữ liệu dashboard thành công.");
            }

            var activeTrackIds = activeTracks.Select(t => t.Id).ToList();
            var trackById = activeTracks.ToDictionary(t => t.Id);

            var totalActiveTeams = await teamRepo.CountAsync(t =>
                !t.IsDeleted && activeTrackIds.Contains(t.TrackId));

            var totalPendingTeams = await teamRepo.CountAsync(
                t => !t.IsDeleted && activeTrackIds.Contains(t.TrackId) &&
                t.Status == TeamConstants.Status.Pending);

            var activeRounds = await roundRepo.GetAllAsync(r => activeTrackIds.Contains(r.TrackId));

            var activeRoundStatuses = activeRounds.Select(round =>
            {
                trackById.TryGetValue(round.TrackId, out var track);

                Event? ev = null;
                if (track is not null)
                    eventById.TryGetValue(track.EventId, out ev);

                return new RoundStatusDto
                {
                    EventName = ev?.Name ?? "Không xác định được sự kiện.",
                    TrackName = track?.Name ?? "Không xác định được Track.",
                    RoundName = round.Name,
                    Status = round.Status
                };
            }).ToList();

            var activeRoundIds = activeRounds.Select(r => r.Id).ToList();

            if (!activeRoundIds.Any())
            {
                var emptyRoundResponse = new CoordinatorDashboardResponse
                {
                    TotalActiveTeams = totalActiveTeams,
                    TotalPendingTeams = totalPendingTeams,
                    ActiveRoundStatuses = activeRoundStatuses,
                    IncompleteSubmissions = 0
                };

                return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                    emptyRoundResponse, "Lấy dữ liệu dashboard thành công.");
            }

            var activeSubmissions = await submissionRepo.GetAllAsync(s =>
                activeRoundIds.Contains(s.RoundId) &&
                !s.IsDisqualified);

            var activeSubmissionIds = activeSubmissions.Select(s => s.Id).ToList();

            var criterionCountByRoundId = await criterionRepo.CountByGroupAsync(
                c => activeRoundIds.Contains(c.RoundId),
                c => c.RoundId);

            var judgeCountByRoundId = await judgeAssignRepo.CountByGroupAsync(
                j => activeRoundIds.Contains(j.RoundId),
                j => j.RoundId);

            var scoreCountBySubmissionId = activeSubmissionIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await scoreRecordRepo.CountByGroupAsync(
                    s => activeSubmissionIds.Contains(s.SubmissionId),
                    s => s.SubmissionId);

            var incompleteSubmissionsCount = 0;

            foreach (var submission in activeSubmissions)
            {
                criterionCountByRoundId.TryGetValue(submission.RoundId, out var criteriaCount);
                judgeCountByRoundId.TryGetValue(submission.RoundId, out var judgeCount);
                scoreCountBySubmissionId.TryGetValue(submission.Id, out var actualScores);

                var expectedScores = criteriaCount * judgeCount;

                if (expectedScores > 0 && actualScores < expectedScores)
                    incompleteSubmissionsCount++;
            }

            var response = new CoordinatorDashboardResponse
            {
                TotalActiveTeams = totalActiveTeams,
                TotalPendingTeams = totalPendingTeams,
                ActiveRoundStatuses = activeRoundStatuses,
                IncompleteSubmissions = incompleteSubmissionsCount
            };

            return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                response, "Lấy dữ liệu dashboard thành công.");
        }
    }
}
