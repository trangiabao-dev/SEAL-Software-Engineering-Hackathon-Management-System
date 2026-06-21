using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using RankingEntity = SealHackathon.Domain.Entities.Ranking;

namespace SealHackathon.Application.Services.Implementations
{
    // Lớp RoundService điều phối tất cả các hoạt động liên quan đến Vòng Thi (Round)
    public class RoundService : IRoundService
    {
        // UnitOfWork dùng để truy cập repository và lưu thay đổi xuống database.
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public RoundService(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        // Hàm lấy ra danh sách các Vòng thi thuộc về một Bảng đấu cụ thể
        public async Task<ApiResponse<List<RoundResponse>>> GetRoundsByTrackIdAsync(int trackId)
        {
            // Kiểm tra Track cha có tồn tại không
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == trackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // Lấy toàn bộ Vòng thi (Ví dụ: Vòng Sơ loại, Vòng Bán kết...) của Track này
            var rounds = await _uow.GetRepository<Round>().GetAllAsync(x => x.TrackId == trackId);

            // Đóng gói sang DTO để trả dữ liệu gọn cho FE.
            var response = rounds.Select(MapToRoundResponse).ToList();

            return ApiResponse<List<RoundResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<List<RoundSelectionResponse>>> GetRoundsForSelectionByEventAsync(int eventId)
        {
            var eventExists = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);
            if (eventExists == null) throw new NotFoundException("Không tìm thấy sự kiện.");

            var tracks = await _uow.GetRepository<Track>().GetAllAsync(t => t.EventId == eventId && !t.IsDeleted);
            var trackIds = tracks.Select(t => t.Id).ToList();

            var rounds = await _uow.GetRepository<Round>().GetAllAsync(r => trackIds.Contains(r.TrackId));

            var result = rounds.Select(r => new { r, trackName = tracks.First(t => t.Id == r.TrackId).Name })
                               .OrderBy(x => x.trackName)
                               .ThenBy(x => x.r.OrderIndex)
                               .Select(x => new RoundSelectionResponse
            {
                Id = x.r.Id,
                Name = x.r.Name,
                TrackId = x.r.TrackId,
                TrackName = x.trackName
            }).ToList();

            return ApiResponse<List<RoundSelectionResponse>>.SuccessResult(result, "Lấy danh sách Round thành công.");
        }

        // Hàm TẠO MỚI một Vòng thi
        public async Task<ApiResponse<RoundResponse>> CreateRoundAsync(CreateRoundRequest request)
        {
            // Kiểm tra tính hợp lệ của Track cha
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == request.TrackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // Khởi tạo Entity Vòng thi
            var newRound = new Round
            {
                TrackId = request.TrackId, // Gắn vào bảng đấu nào
                Name = request.Name,
                OrderIndex = request.OrderIndex,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                AdvancingSlots = request.AdvancingSlots,
                Status = RoundConstants.Status.Upcoming, // Mới tạo ra thì mặc định là trạng thái "Sắp diễn ra"
                CreatedAt = DateTime.UtcNow
            };

            // Lưu thẳng xuống DB
            await _uow.GetRepository<Round>().AddAsync(newRound);
            await _uow.SaveChangesAsync();

            // Chuyển sang DTO trả về kết quả
            var response = MapToRoundResponse(newRound);

            return ApiResponse<RoundResponse>.SuccessResult(response, "Tạo Round thành công.");
        }

        // Hàm CẬP NHẬT thông tin Vòng thi (Chỉ đổi Tên, Thời gian, Số slot đi tiếp...)
        public async Task<ApiResponse<RoundResponse>> UpdateRoundAsync(int id, UpdateRoundRequest request)
        {
            var existingRound = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == id);
            if (existingRound == null) throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            // Đè thông tin mới
            existingRound.Name = request.Name;
            existingRound.OrderIndex = request.OrderIndex;
            existingRound.StartTime = request.StartTime;
            existingRound.EndTime = request.EndTime;
            existingRound.AdvancingSlots = request.AdvancingSlots;
            existingRound.UpdatedAt = DateTime.UtcNow;

            _uow.GetRepository<Round>().Update(existingRound);
            await _uow.SaveChangesAsync();

            var response = MapToRoundResponse(existingRound);

            return ApiResponse<RoundResponse>.SuccessResult(response, "Cập nhật thông tin Round thành công.");
        }

        // Hàm ĐỔI TRẠNG THÁI Vòng thi.
        // Khi Coordinator chuyển Round sang Active, hệ thống sẽ tự động gán Topic cho các team đã được duyệt.
        public async Task<ApiResponse<RoundResponse>> UpdateRoundStatusAsync(int id, UpdateRoundStatusRequest request)
        {
            // Chuẩn hóa status FE gửi lên.
            // Ví dụ FE gửi "active" thì BE lưu thống nhất là "Active".
            // Nếu FE gửi status không nằm trong danh sách hợp lệ thì báo lỗi.
            var newStatus = NormalizeRoundStatus(request.Status);

            var roundRepo = _uow.GetRepository<Round>();

            var existingRound = await roundRepo.GetFirstOrDefaultTrackingAsync(x => x.Id == id);
            if (existingRound is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            // Nếu status mới giống status hiện tại thì không xử lý lại.
            // Quan trọng: tránh trường hợp bấm Active nhiều lần làm random topic lại.
            if (string.Equals(existingRound.Status, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<RoundResponse>.SuccessResult(
                    MapToRoundResponse(existingRound),
                    "Trạng thái Round không thay đổi.");
            }

            var statusOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { RoundConstants.Status.Upcoming, 0 },
                { RoundConstants.Status.Active, 1 },
                { RoundConstants.Status.Scoring, 2 },
                { RoundConstants.Status.Closed, 3 }
            };

            if (statusOrder.TryGetValue(existingRound.Status, out int currentOrder) && 
                statusOrder.TryGetValue(newStatus, out int newOrder))
            {
                if (newOrder < currentOrder)
                {
                    throw new BadRequestException("Không thể lùi trạng thái của vòng thi về trạng thái trước đó.");
                }
            }

            // Quy tắc: Chỉ khi chuyển Round sang Active thì mới gán đề cho team.
            // Các trạng thái khác như Scoring, Closed chỉ đổi status, không random topic.
            if (string.Equals(newStatus, RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
            {
                // Fix lỗ hổng: Không cho phép 2 Round cùng Active trong 1 Track
                var activeRoundExists = await roundRepo.GetFirstOrDefaultAsync(r => 
                    r.TrackId == existingRound.TrackId && 
                    r.Id != existingRound.Id && 
                    r.Status == RoundConstants.Status.Active);
                
                if (activeRoundExists != null)
                {
                    throw new BadRequestException("Không thể kích hoạt vòng thi này vì đang có một vòng thi khác đang diễn ra (Active) trong cùng một bảng đấu (Track). Vui lòng đóng vòng thi hiện tại trước.");
                }

                await EnsureEventIsActiveBeforeStartingRoundAsync(existingRound);
                await AssignTopicsForRoundAsync(existingRound);
            }

            if (string.Equals(newStatus, RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(newStatus, RoundConstants.Status.Scoring, StringComparison.OrdinalIgnoreCase))
            {
                var criteria = await _uow.GetRepository<Criterion>().GetAllAsync(c => c.RoundId == id);
                if (!criteria.Any())
                {
                    throw new BadRequestException(ErrorMessages.Ranking.RoundHasNoCriteria);
                }

                var totalWeight = criteria.Sum(c => c.Weight);
                if (Math.Abs(totalWeight - 1.0) > 0.0001)
                {
                    throw new BadRequestException("Tổng trọng số của các tiêu chí phải đúng 100% trước khi kích hoạt vòng thi hoặc chuyển sang chấm điểm.");
                }
            }

            existingRound.Status = newStatus;
            existingRound.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            return ApiResponse<RoundResponse>.SuccessResult(
                MapToRoundResponse(existingRound),
                "Cập nhật trạng thái Round thành công.");
        }

        // Hàm gán Judge vào Round.
        // Chỉ tài khoản đã có EventRole = Judge và Status = Approved trong Event của Round mới được gán.
        public async Task<ApiResponse<bool>> AssignJudgeAsync(int roundId, AssignJudgeRequest request, Guid assignedBy)
        {
            if (roundId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidRoundId);

            if (request.JudgeId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidJudgeId);

            // Tìm Round cần gán Judge.
            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            // Lấy Track để biết Round này thuộc Event nào.
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // Kiểm tra tài khoản Judge còn tồn tại và chưa bị xóa mềm.
            var judge = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == request.JudgeId && !a.IsDeleted);

            if (judge is null)
                throw new NotFoundException(ErrorMessages.Common.JudgeNotFound);

            // Judge phải được phân quyền trong đúng Event của Round.
            // Không chỉ có account là đủ, phải có EventAccount với EventRole = Judge.
            var judgeEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == request.JudgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == EventAccountConstants.Status.Approved);

            if (judgeEventRole is null)
                throw new BadRequestException(ErrorMessages.Common.JudgeNotInEvent);

            // Không cho gán trùng cùng một Judge vào cùng một Round.
            var existingAssign = await _uow.GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == request.JudgeId
                                           && ja.RoundId == roundId);

            if (existingAssign is not null)
                throw new ConflictException(ErrorMessages.Round.JudgeAlreadyAssigned);

            var judgeAssign = new JudgeAssign
            {
                JudgeId = request.JudgeId,
                RoundId = roundId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            };

            await _uow.GetRepository<JudgeAssign>().AddAsync(judgeAssign);
            await _uow.SaveChangesAsync();

            // Gửi thông báo cho Giám khảo
            await _notificationService.SendNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
            {
                AccountId = request.JudgeId,
                Title = "Phân công Giám khảo",
                Message = $"Bạn vừa được phân công làm giám khảo cho vòng thi {round.Name}.",
                Type = "JUDGE_ASSIGNED"
            });

            return ApiResponse<bool>.SuccessResult(true, "Phân công Judge vào Round thành công.");
        }


        // [DEV 1 - LẤY DANH SÁCH GIÁM KHẢO CỦA VÒNG THI]
        // Chức năng: Trả về danh sách chi tiết các giám khảo (kèm email, username, loại giám khảo) đã được phân công vào vòng thi này.
        public async Task<ApiResponse<List<RoundJudgeResponse>>> GetJudgesByRoundAsync(int roundId)
        {
            var round = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(r => r.Id == roundId);
            if (round == null) throw new NotFoundException($"Không tìm thấy Round với ID {roundId}");

            var judgeAssigns = await _uow.GetRepository<JudgeAssign>()
                .GetAllAsync(ja => ja.RoundId == roundId);

            var judgeIds = judgeAssigns.Select(ja => ja.JudgeId).Distinct().ToList();
            
            // Khắc phục lỗi "OPENJSON" của EF Core 8 trên SQL Server cũ khi dùng Contains
            var judges = new List<Account>();
            foreach (var id in judgeIds)
            {
                var acc = await _uow.GetRepository<Account>().GetFirstOrDefaultAsync(a => a.Id == id);
                if (acc != null)
                {
                    judges.Add(acc);
                }
            }

            var track = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(t => t.Id == round.TrackId);
            var eventAccounts = await _uow.GetRepository<EventAccount>()
                .GetAllAsync(ea => ea.EventId == track!.EventId && ea.EventRole == RoleConstants.Judge);

            var response = judgeAssigns.Select(ja => {
                var ea = eventAccounts.FirstOrDefault(e => e.AccountId == ja.JudgeId);
                var judgeAcc = judges.FirstOrDefault(j => j.Id == ja.JudgeId);
                return new RoundJudgeResponse
                {
                    JudgeId = ja.JudgeId,
                    Email = judgeAcc?.Email ?? "",
                    Username = judgeAcc?.Username ?? "",
                    JudgeType = ea?.JudgeType,
                    AssignedAt = ja.AssignedAt
                };
            }).ToList();

            return ApiResponse<List<RoundJudgeResponse>>.SuccessResult(response);
        }


        // =============== Private helpers ===============
        // Kiểm tra và chuẩn hóa status của Round.
        // BE chỉ cho phép 4 trạng thái: Upcoming, Active, Scoring, Closed.
        private static string NormalizeRoundStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);

            status = status.Trim();

            if (string.Equals(status, RoundConstants.Status.Upcoming, StringComparison.OrdinalIgnoreCase))
                return RoundConstants.Status.Upcoming;

            if (string.Equals(status, RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
                return RoundConstants.Status.Active;

            if (string.Equals(status, RoundConstants.Status.Scoring, StringComparison.OrdinalIgnoreCase))
                return RoundConstants.Status.Scoring;

            if (string.Equals(status, RoundConstants.Status.Closed, StringComparison.OrdinalIgnoreCase))
                return RoundConstants.Status.Closed;

            throw new BadRequestException(ErrorMessages.Common.InvalidStatus);
        }

        // Gán Topic cho các team đã được duyệt trong Track của Round.
        // Team đã có Topic thì giữ nguyên, không random lại.
        private async Task EnsureEventIsActiveBeforeStartingRoundAsync(Round round)
        {
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            var eventEntity = await _uow.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == track.EventId && !e.IsDeleted);

            if (eventEntity is null)
                throw new NotFoundException(ErrorMessages.Event.CurrentEventNotFound);

            if (!string.Equals(eventEntity.Status, EventConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.Round.EventMustBeActiveToStartRound);
        }

        private async Task AssignTopicsForRoundAsync(Round round)
        {
            var roundTeamRepo = _uow.GetRepository<RoundTeam>();

            var existingRoundTeams = await roundTeamRepo.GetAllAsync(rt => rt.RoundId == round.Id);
            if (existingRoundTeams.Any())
                return;

            var teams = await GetTeamsQualifiedForRoundAsync(round);
            if (!teams.Any())
                throw new BadRequestException(ErrorMessages.Round.NoTeamQualifiedForRound);

            var topic = await ResolveTopicForRoundAsync(round, teams);

            var now = DateTime.UtcNow;
            var teamRepo = _uow.GetRepository<Team>();

            foreach (var team in teams)
            {
                await roundTeamRepo.AddAsync(new RoundTeam
                {
                    Id = Guid.NewGuid(),
                    RoundId = round.Id,
                    TeamId = team.Id,
                    TopicId = topic.Id,
                    AssignedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                team.TopicId = topic.Id;
                team.UpdatedAt = now;
                teamRepo.Update(team);
            }
        }

        private async Task<List<Team>> GetTeamsQualifiedForRoundAsync(Round round)
        {
            var teams = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.TrackId == round.TrackId
                               && t.Status == TeamConstants.Status.Approved
                               && !t.IsDeleted);

            var previousRound = await GetPreviousRoundAsync(round);
            if (previousRound is null)
                return teams;

            EnsurePreviousRoundClosed(previousRound);

            var advancingRankings = await _uow.GetRepository<RankingEntity>()
                .GetAllAsync(r => r.RoundId == previousRound.Id && r.IsAdvancing);

            if (!advancingRankings.Any())
                throw new BadRequestException(ErrorMessages.Round.PreviousRoundRankingRequired);

            var advancingTeamIds = advancingRankings.Select(r => r.TeamId).ToHashSet();

            return teams.Where(t => advancingTeamIds.Contains(t.Id)).ToList();
        }

        /// <summary>
        /// Đảm bảo Round trước đã đóng trước khi mở Round tiếp theo.
        /// </summary>
        private static void EnsurePreviousRoundClosed(Round previousRound)
        {
            if (!string.Equals(previousRound.Status, RoundConstants.Status.Closed, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(ErrorMessages.Round.PreviousRoundMustBeClosed);
        }

        private async Task<Round?> GetPreviousRoundAsync(Round round)
        {
            var rounds = await _uow.GetRepository<Round>()
                .GetAllAsync(r => r.TrackId == round.TrackId && r.OrderIndex < round.OrderIndex);

            return rounds
                .OrderByDescending(r => r.OrderIndex)
                .FirstOrDefault();
        }

        private async Task<Topic> ResolveTopicForRoundAsync(Round round, List<Team> teams)
        {
            var roundTopics = await _uow.GetRepository<Topic>()
                .GetAllAsync(t => t.RoundId == round.Id);

            if (roundTopics.Any())
                return roundTopics.OrderBy(_ => Random.Shared.Next()).First();

            var existingTopicIds = teams
                .Where(t => t.TopicId.HasValue)
                .Select(t => t.TopicId!.Value)
                .Distinct()
                .ToList();

            if (existingTopicIds.Count == 1)
            {
                var existingTopic = await _uow.GetRepository<Topic>()
                    .GetFirstOrDefaultAsync(t => t.Id == existingTopicIds[0]);

                if (existingTopic is not null)
                    return existingTopic;
            }

            throw new BadRequestException(ErrorMessages.Round.NoTopicToAssign);
        }

        public async Task<ApiResponse<List<JudgeAssignedRoundResponse>>> GetAssignedRoundsForJudgeAsync(Guid judgeId)
        {
            var judgeAssigns = await _uow.GetRepository<JudgeAssign>()
                .GetAllAsync(ja => ja.JudgeId == judgeId);

            var roundIds = judgeAssigns.Select(ja => ja.RoundId).Distinct().ToList();

            var rounds = await _uow.GetRepository<Round>()
                .GetAllAsync(r => roundIds.Contains(r.Id));

            var trackIds = rounds.Select(r => r.TrackId).Distinct().ToList();
            var tracks = await _uow.GetRepository<Track>()
                .GetAllAsync(t => trackIds.Contains(t.Id));

            var eventIds = tracks.Select(t => t.EventId).Distinct().ToList();
            var events = await _uow.GetRepository<Event>()
                .GetAllAsync(e => eventIds.Contains(e.Id));

            // Đếm số lượng Submission trong các Round này
            var submissions = await _uow.GetRepository<Submission>()
                .GetAllAsync(s => roundIds.Contains(s.RoundId));
            
            var submissionCounts = submissions.GroupBy(s => s.RoundId)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<JudgeAssignedRoundResponse>();
            foreach (var round in rounds)
            {
                var track = tracks.FirstOrDefault(t => t.Id == round.TrackId);
                var ev = track != null ? events.FirstOrDefault(e => e.Id == track.EventId) : null;
                
                result.Add(new JudgeAssignedRoundResponse
                {
                    Id = round.Id,
                    Name = round.Name,
                    TrackName = track?.Name ?? "Unknown Track",
                    EventName = ev?.Name ?? "Unknown Event",
                    StartTime = round.StartTime,
                    EndTime = round.EndTime,
                    Status = round.Status,
                    SubmissionCount = submissionCounts.ContainsKey(round.Id) ? submissionCounts[round.Id] : 0
                });
            }

            return ApiResponse<List<JudgeAssignedRoundResponse>>.SuccessResult(result, "Lấy danh sách vòng thi được phân công thành công.");
        }

        // Chuyển Round entity sang RoundResponse để trả về cho FE.
        private static RoundResponse MapToRoundResponse(Round round)
        {
            return new RoundResponse
            {
                Id = round.Id,
                TrackId = round.TrackId,
                Name = round.Name,
                OrderIndex = round.OrderIndex,
                StartTime = round.StartTime,
                EndTime = round.EndTime,
                AdvancingSlots = round.AdvancingSlots,
                Status = round.Status
            };
        }
    }
}
