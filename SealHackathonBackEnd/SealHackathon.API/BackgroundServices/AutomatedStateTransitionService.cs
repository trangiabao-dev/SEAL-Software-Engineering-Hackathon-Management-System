using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SealHackathon.API.BackgroundServices
{
    public class AutomatedStateTransitionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutomatedStateTransitionService> _logger;

        public AutomatedStateTransitionService(IServiceProvider serviceProvider, ILogger<AutomatedStateTransitionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Automated State Transition Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessTransitionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing state transitions.");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            _logger.LogInformation("Automated State Transition Service is stopping.");
        }

        private async Task ProcessTransitionsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var roundService = scope.ServiceProvider.GetRequiredService<IRoundService>();
            
            var now = DateTime.UtcNow;

            // 1. Kích hoạt Event (Registration -> Active) nếu đến giờ
            var eventsToActivate = await uow.GetRepository<Event>().GetAllAsync(
                e => !e.IsDeleted && e.Status == EventConstants.Status.Registration && e.StartDate <= now);

            foreach (var evt in eventsToActivate)
            {
                try
                {
                    // Check Guard Clauses
                    var approvedTeams = await uow.GetRepository<Team>().GetAllAsync(t => t.Track.EventId == evt.Id && t.Status == TeamConstants.Status.Approved && !t.IsDeleted);
                    if (approvedTeams.Any(t => t.MentorId == null))
                    {
                        _logger.LogWarning($"Event {evt.Id} cannot be auto-activated: Missing Mentors for approved teams.");
                        continue;
                    }

                    var tracks = await uow.GetRepository<Track>().GetAllAsync(t => t.EventId == evt.Id && !t.IsDeleted);
                    var trackIds = tracks.Select(t => t.Id).ToList();
                    var rounds = await uow.GetRepository<Round>().GetAllAsync(r => trackIds.Contains(r.TrackId));
                    var judgeAssigns = await uow.GetRepository<JudgeAssign>().GetAllAsync(ja => rounds.Select(r => r.Id).Contains(ja.RoundId));

                    bool missingJudges = false;
                    foreach (var round in rounds)
                    {
                        if (!judgeAssigns.Any(ja => ja.RoundId == round.Id))
                        {
                            missingJudges = true;
                            break;
                        }
                    }

                    if (missingJudges)
                    {
                        _logger.LogWarning($"Event {evt.Id} cannot be auto-activated: Missing Judges for rounds.");
                        continue;
                    }

                    evt.Status = EventConstants.Status.Active;
                    evt.UpdatedAt = now;
                    uow.GetRepository<Event>().Update(evt);
                    await uow.SaveChangesAsync();
                    
                    _logger.LogInformation($"Auto-activated Event {evt.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to auto-activate Event {evt.Id}");
                }
            }

            // 2. Đóng nộp bài Round (Active -> Scoring) nếu đến giờ
            var roundsToScore = await uow.GetRepository<Round>().GetAllAsync(
                r => r.Status == RoundConstants.Status.Active && r.EndTime <= now);

            foreach (var round in roundsToScore)
            {
                try
                {
                    await roundService.UpdateRoundStatusAsync(round.Id, new UpdateRoundStatusRequest { Status = RoundConstants.Status.Scoring });
                    _logger.LogInformation($"Auto-moved Round {round.Id} to Scoring");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to auto-move Round {round.Id} to Scoring");
                }
            }

            // 3. Kích hoạt Round (Upcoming -> Active) nếu đến giờ và Event đang Active
            var roundsToActivate = await uow.GetRepository<Round>().GetAllAsync(
                r => r.Status == RoundConstants.Status.Upcoming && r.StartTime <= now);

            foreach (var round in roundsToActivate)
            {
                try
                {
                    // Lấy Track và Event để check Event đã Active chưa
                    var track = await uow.GetRepository<Track>().GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);
                    if (track != null)
                    {
                        var evt = await uow.GetRepository<Event>().GetFirstOrDefaultAsync(e => e.Id == track.EventId && !e.IsDeleted);
                        if (evt != null && evt.Status == EventConstants.Status.Active)
                        {
                            // Kiểm tra vòng trước đã đóng chưa (để không văng lỗi Console mỗi 60s)
                            var previousRound = await uow.GetRepository<Round>()
                                .GetAllAsync(r => r.TrackId == round.TrackId && r.OrderIndex < round.OrderIndex);
                            var lastPreviousRound = previousRound.OrderByDescending(r => r.OrderIndex).FirstOrDefault();

                            if (lastPreviousRound != null && lastPreviousRound.Status != RoundConstants.Status.Closed)
                            {
                                _logger.LogWarning($"Cannot auto-activate Round {round.Id}: Previous round {lastPreviousRound.Id} is not Closed yet.");
                                continue;
                            }

                            await roundService.UpdateRoundStatusAsync(round.Id, new UpdateRoundStatusRequest { Status = RoundConstants.Status.Active });
                            _logger.LogInformation($"Auto-activated Round {round.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to auto-activate Round {round.Id}");
                }
            }
        }
    }
}
