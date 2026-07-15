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
            var judgeAssignRepo = _uow.GetRepository<JudgeAssign>();
            var mentorAssignRepo = _uow.GetRepository<MentorAssign>();
            var criterionRepo = _uow.GetRepository<Criterion>();
            var scoreRecordRepo = _uow.GetRepository<ScoreRecord>();

            var allEvents = await eventRepo.GetAllAsync(e => !e.IsDeleted);

            var response = new CoordinatorDashboardResponse();
            if (!allEvents.Any())
            {
                return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                    response, "Lấy dữ liệu dashboard thành công.");
            }

            var eventIds = allEvents.Select(e => e.Id).ToList();
            var allTracks = await trackRepo.GetAllAsync(t => !t.IsDeleted && eventIds.Contains(t.EventId));
            var trackIds = allTracks.Select(t => t.Id).ToList();

            var allRounds = await roundRepo.GetAllAsync(r => trackIds.Contains(r.TrackId));
            var roundIds = allRounds.Select(r => r.Id).ToList();

            var allTeams = await teamRepo.GetAllAsync(t => !t.IsDeleted && trackIds.Contains(t.TrackId));
            var allSubmissions = await submissionRepo.GetAllAsync(s => roundIds.Contains(s.RoundId));

            var judgeAssigns = await judgeAssignRepo.GetAllAsync(j => roundIds.Contains(j.RoundId));
            var mentorAssigns = await mentorAssignRepo.GetAllAsync(m => trackIds.Contains(m.TrackId));

            // Populate Summary
            response.Summary.TotalEvents = allEvents.Count;
            response.Summary.TotalTeams = allTeams.Count;
            response.Summary.TotalSubmissions = allSubmissions.Count;
            response.Summary.TotalJudges = judgeAssigns.Select(j => j.JudgeId).Distinct().Count();
            response.Summary.TotalMentors = mentorAssigns.Select(m => m.MentorId).Distinct().Count();

            response.Summary.EventsByStatus = allEvents.GroupBy(e => e.Status)
                .ToDictionary(g => g.Key, g => g.Count());
            response.Summary.RoundsByStatus = allRounds.GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());
            response.Summary.TeamsByStatus = allTeams.GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Pre-calculate incomplete submissions
            var activeSubmissions = allSubmissions.Where(s => !s.IsDisqualified).ToList();
            var activeSubmissionIds = activeSubmissions.Select(s => s.Id).ToList();
            var criterionCountByRoundId = await criterionRepo.CountByGroupAsync(
                c => roundIds.Contains(c.RoundId), c => c.RoundId);
            var judgeCountByRoundId = await judgeAssignRepo.CountByGroupAsync(
                j => roundIds.Contains(j.RoundId), j => j.RoundId);
            var scoreCountBySubmissionId = activeSubmissionIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await scoreRecordRepo.CountByGroupAsync(
                    s => activeSubmissionIds.Contains(s.SubmissionId), s => s.SubmissionId);

            var incompleteSubmissionIds = new HashSet<Guid>();
            foreach (var submission in activeSubmissions)
            {
                criterionCountByRoundId.TryGetValue(submission.RoundId, out var criteriaCount);
                judgeCountByRoundId.TryGetValue(submission.RoundId, out var judgeCount);
                scoreCountBySubmissionId.TryGetValue(submission.Id, out var actualScores);

                var expectedScores = criteriaCount * judgeCount;
                if (expectedScores > 0 && actualScores < expectedScores)
                {
                    incompleteSubmissionIds.Add(submission.Id);
                }
            }

            response.Summary.IncompleteSubmissions = incompleteSubmissionIds.Count;

            // Generate Event Details and Charts
            var teamCountByEventId = new Dictionary<int, int>();
            foreach (var ev in allEvents)
            {
                var evTracks = allTracks.Where(t => t.EventId == ev.Id).ToList();
                var evTrackIds = evTracks.Select(t => t.Id).ToList();
                var evRounds = allRounds.Where(r => evTrackIds.Contains(r.TrackId)).ToList();
                var evRoundIds = evRounds.Select(r => r.Id).ToList();
                var evTeams = allTeams.Where(t => evTrackIds.Contains(t.TrackId)).ToList();
                var evSubmissions = allSubmissions.Where(s => evRoundIds.Contains(s.RoundId)).ToList();
                var evIncompleteSubmissions = evSubmissions.Count(s => incompleteSubmissionIds.Contains(s.Id));

                var eventDto = new DashboardEventDto
                {
                    EventId = ev.Id,
                    EventName = ev.Name,
                    Status = ev.Status,
                    StartDate = ev.StartDate,
                    EndDate = ev.EndDate,
                    TotalTracks = evTracks.Count,
                    TotalRounds = evRounds.Count,
                    TotalTeams = evTeams.Count,
                    ApprovedTeams = evTeams.Count(t => t.Status == TeamConstants.Status.Approved),
                    PendingTeams = evTeams.Count(t => t.Status == TeamConstants.Status.Pending),
                    RejectedTeams = evTeams.Count(t => t.Status == TeamConstants.Status.Rejected),
                    DisqualifiedTeams = evTeams.Count(t => t.Status == TeamConstants.Status.Disqualified || string.Equals(t.Status, "Disqualified", StringComparison.OrdinalIgnoreCase)),
                    TotalSubmissions = evSubmissions.Count,
                    IncompleteSubmissions = evIncompleteSubmissions,
                    ActiveRounds = evRounds.Count(r => r.Status == RoundConstants.Status.Active),
                    ScoringRounds = evRounds.Count(r => r.Status == RoundConstants.Status.Scoring),
                    ClosedRounds = evRounds.Count(r => r.Status == RoundConstants.Status.Closed),
                    ResultsAvailable = evRounds.Any(r => r.Status == RoundConstants.Status.Closed)
                };

                response.Events.Add(eventDto);
                teamCountByEventId[ev.Id] = evTeams.Count;

                // Charts: Event Teams & Submissions
                response.Charts.TeamCountByEvent.Add(new ChartTeamByEventDto
                {
                    EventId = ev.Id,
                    EventName = ev.Name,
                    TotalTeams = evTeams.Count
                });
                response.Charts.SubmissionCountByEvent.Add(new ChartSubmissionByEventDto
                {
                    EventId = ev.Id,
                    EventName = ev.Name,
                    TotalSubmissions = evSubmissions.Count
                });

                // Charts: Track Teams
                foreach (var track in evTracks)
                {
                    var trackTeams = evTeams.Count(t => t.TrackId == track.Id);
                    response.Charts.TeamCountByTrack.Add(new ChartTeamByTrackDto
                    {
                        EventId = ev.Id,
                        TrackId = track.Id,
                        TrackName = track.Name,
                        TotalTeams = trackTeams
                    });
                }
            }

            // Find Highlight
            var highlightEvent = response.Events.OrderByDescending(e => e.TotalTeams).FirstOrDefault();
            if (highlightEvent != null && highlightEvent.TotalTeams > 0)
            {
                response.Highlight.EventWithMostTeams = new DashboardHighlightEventDto
                {
                    EventId = highlightEvent.EventId,
                    EventName = highlightEvent.EventName,
                    TotalTeams = highlightEvent.TotalTeams
                };
            }

            return ApiResponse<CoordinatorDashboardResponse>.SuccessResult(
                response, "Lấy dữ liệu dashboard thành công.");
        }

        public async Task<ApiResponse<MentorDashboardResponse>> GetMentorDashboardAsync(Guid mentorId)
        {
            var teamRepo = _uow.GetRepository<Team>();
            var roundRepo = _uow.GetRepository<Round>();
            var roundTeamRepo = _uow.GetRepository<RoundTeam>();
            var submissionRepo = _uow.GetRepository<Submission>();
            var topicRepo = _uow.GetRepository<Topic>();

            // 1. Lấy danh sách các Team mà Mentor này đang quản lý, bao gồm Track và Event đang active
            var teams = await teamRepo.GetAllAsync(t => 
                t.MentorId == mentorId && 
                !t.IsDeleted && 
                (t.Track.Event.Status == EventConstants.Status.Active || t.Track.Event.Status == EventConstants.Status.Registration));

            var response = new MentorDashboardResponse();

            foreach (var team in teams)
            {
                var dto = new MentorDashboardTeamDto
                {
                    TeamId = team.Id,
                    TeamName = team.TeamName,
                    TrackName = team.Track?.Name ?? "N/A"
                };

                // 2. Tìm Round đang active của Track này
                var activeRounds = await roundRepo.GetAllAsync(r => 
                    r.TrackId == team.TrackId && 
                    r.Status == RoundConstants.Status.Active);
                
                var activeRound = activeRounds.OrderBy(r => r.OrderIndex).FirstOrDefault();

                if (activeRound != null)
                {
                    dto.ActiveRoundName = activeRound.Name;
                    dto.ActiveRoundStatus = activeRound.Status;

                    // 3. Tìm Topic của Team trong Round này
                    var roundTeam = await roundTeamRepo.GetFirstOrDefaultAsync(rt => 
                        rt.RoundId == activeRound.Id && 
                        rt.TeamId == team.Id && 
                        rt.TopicId != null);

                    if (roundTeam != null && roundTeam.TopicId.HasValue)
                    {
                        var topic = await topicRepo.GetFirstOrDefaultAsync(tp => tp.Id == roundTeam.TopicId.Value);
                        dto.TopicName = topic?.Title;
                    }

                    // 4. Lấy trạng thái Submission của Team trong Round này
                    var submission = await submissionRepo.GetFirstOrDefaultAsync(s => 
                        s.RoundId == activeRound.Id && 
                        s.TeamId == team.Id && 
                        !s.IsDisqualified);

                    if (submission != null)
                    {
                        dto.IsSubmitted = true;
                        dto.SubmittedAt = submission.CreatedAt;
                    }
                    else
                    {
                        dto.IsSubmitted = false;
                    }
                }

                response.Teams.Add(dto);
            }

            return ApiResponse<MentorDashboardResponse>.SuccessResult(response, "Lấy dữ liệu Mentor Dashboard thành công.");
        }
    }
}
