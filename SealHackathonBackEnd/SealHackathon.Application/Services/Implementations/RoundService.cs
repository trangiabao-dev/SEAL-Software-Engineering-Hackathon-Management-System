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
            if (request.OrderIndex <= 0)
                throw new BadRequestException("Thứ tự vòng thi (OrderIndex) phải lớn hơn 0.");

            // Kiểm tra tính hợp lệ của Track cha
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == request.TrackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            if (request.AdvancingSlots.HasValue && request.AdvancingSlots.Value <= 0)
            {
                throw new BadRequestException("Số lượng đội đi tiếp phải là số dương hoặc để trống (vòng chung kết).");
            }

            // Kiểm tra trùng lặp OrderIndex trong cùng một Track
            var existingRoundWithSameOrder = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(x => x.TrackId == request.TrackId && x.OrderIndex == request.OrderIndex);
            if (existingRoundWithSameOrder != null)
            {
                throw new BadRequestException("Thứ tự vòng thi (OrderIndex) đã tồn tại trong bảng đấu này.");
            }

            // Kiểm tra mỗi Track chỉ có tối đa 1 vòng chung kết (AdvancingSlots rỗng)
            if (!request.AdvancingSlots.HasValue)
            {
                var existingFinalRound = await _uow.GetRepository<Round>()
                    .GetFirstOrDefaultAsync(x => x.TrackId == request.TrackId && x.AdvancingSlots == null);
                if (existingFinalRound != null)
                {
                    throw new BadRequestException("Mỗi bảng đấu chỉ được phép có tối đa 1 vòng chung kết (để trống số lượng đi tiếp).");
                }
            }

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
            if (request.OrderIndex <= 0)
                throw new BadRequestException("Thứ tự vòng thi (OrderIndex) phải lớn hơn 0.");

            var existingRound = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == id);
            if (existingRound == null) throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            if (request.AdvancingSlots.HasValue && request.AdvancingSlots.Value <= 0)
            {
                throw new BadRequestException("Số lượng đội đi tiếp phải là số dương hoặc để trống (vòng chung kết).");
            }

            // Kiểm tra trùng lặp OrderIndex
            var existingRoundWithSameOrder = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(x => x.TrackId == existingRound.TrackId && x.OrderIndex == request.OrderIndex && x.Id != id);
            if (existingRoundWithSameOrder != null)
            {
                throw new BadRequestException("Thứ tự vòng thi (OrderIndex) đã tồn tại trong bảng đấu này.");
            }

            // Kiểm tra mỗi Track chỉ có tối đa 1 vòng chung kết
            if (!request.AdvancingSlots.HasValue)
            {
                var existingFinalRound = await _uow.GetRepository<Round>()
                    .GetFirstOrDefaultAsync(x => x.TrackId == existingRound.TrackId && x.AdvancingSlots == null && x.Id != id);
                if (existingFinalRound != null)
                {
                    throw new BadRequestException("Mỗi bảng đấu chỉ được phép có tối đa 1 vòng chung kết (để trống số lượng đi tiếp).");
                }
            }

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
                
                if (newOrder - currentOrder > 1)
                {
                    throw new BadRequestException("Không thể nhảy cóc trạng thái của vòng thi.");
                }
            }

            if (string.Equals(newStatus, RoundConstants.Status.Closed, StringComparison.OrdinalIgnoreCase))
            {
                var rankings = await _uow.GetRepository<Domain.Entities.Ranking>().GetAllAsync(r => r.RoundId == id);
                if (!rankings.Any())
                {
                    throw new BadRequestException("Không thể đóng vòng thi khi chưa có bảng xếp hạng (Ranking).");
                }

                var orderedRankings = rankings.OrderBy(r => r.RankPosition).ToList();

                if (existingRound.AdvancingSlots.HasValue)
                {
                    var cutoff = existingRound.AdvancingSlots.Value;
                    if (cutoff > 0 && cutoff < orderedRankings.Count)
                    {
                        var lastSelectedTeam = orderedRankings[cutoff - 1];
                        var firstExcludedTeam = orderedRankings[cutoff];
                        if (lastSelectedTeam.RankPosition == firstExcludedTeam.RankPosition)
                        {
                            throw new BadRequestException("Vẫn còn đồng hạng tại ranh giới đi tiếp chưa được giải quyết (Tie-break). Không thể đóng vòng thi.");
                        }
                    }
                }
                else
                {
                    var top3Rankings = orderedRankings.Where(r => r.RankPosition <= 3).ToList();
                    var duplicates = top3Rankings.GroupBy(r => r.RankPosition).Where(g => g.Count() > 1).ToList();
                    if (duplicates.Any())
                    {
                        throw new BadRequestException("Vẫn còn đồng hạng trong Top 3 (nhóm đạt giải) chưa được giải quyết. Không thể đóng vòng thi chung kết.");
                    }
                }

                // Gửi thông báo cho Leader của các đội
                var teamIds = orderedRankings.Select(r => r.TeamId).Distinct().ToList();
                var teams = await _uow.GetRepository<Team>().GetAllAsync(t => teamIds.Contains(t.Id));
                var teamDict = teams.ToDictionary(t => t.Id, t => t);

                var now = DateTime.UtcNow;
                foreach (var ranking in orderedRankings)
                {
                    if (teamDict.TryGetValue(ranking.TeamId, out var team))
                    {
                        string message;
                        string type;
                        
                        if (existingRound.AdvancingSlots.HasValue)
                        {
                            if (ranking.IsAdvancing)
                            {
                                message = $"Chúc mừng đội thi {team.TeamName} đã vượt qua vòng {existingRound.Name} và tiến vào vòng tiếp theo!";
                                type = "ROUND_ADVANCED";
                            }
                            else
                            {
                                message = $"Rất tiếc, đội thi {team.TeamName} đã dừng bước tại vòng {existingRound.Name}. Cảm ơn các bạn đã tham gia!";
                                type = "ROUND_ELIMINATED";
                            }
                        }
                        else
                        {
                            // Vòng chung kết
                            if (ranking.RankPosition <= 3)
                            {
                                message = $"Chúc mừng đội thi {team.TeamName} đã xuất sắc đạt Hạng {ranking.RankPosition} tại vòng chung kết {existingRound.Name}!";
                                type = "FINAL_WINNER";
                            }
                            else
                            {
                                message = $"Đội thi {team.TeamName} đã hoàn thành vòng chung kết {existingRound.Name} với thứ hạng {ranking.RankPosition}. Cảm ơn các bạn đã tham gia!";
                                type = "FINAL_COMPLETED";
                            }
                        }

                        await _uow.GetRepository<Notification>().AddAsync(new Notification
                        {
                            AccountId = team.LeaderId,
                            Title = "Kết quả vòng thi: " + existingRound.Name,
                            Message = message,
                            Type = type,
                            IsRead = false,
                            CreatedAt = now
                        });
                    }
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

            // Lấy danh sách các đội hợp lệ để thi đấu vòng này.
            var teams = await GetTeamsQualifiedForRoundAsync(round);
            if (!teams.Any())
                throw new BadRequestException(ErrorMessages.Round.NoTeamQualifiedForRound);

            // Nếu đã gán rồi thì kiểm tra xem danh sách cũ có khớp với đội được đi tiếp hay không.
            var existingRoundTeams = await roundTeamRepo.GetAllAsync(rt => rt.RoundId == round.Id);
            if (existingRoundTeams.Any())
            {
                var existingTeamIds = existingRoundTeams.Select(rt => rt.TeamId).ToHashSet();
                var qualifiedTeamIds = teams.Select(t => t.Id).ToHashSet();

                if (!existingTeamIds.SetEquals(qualifiedTeamIds))
                {
                    throw new BadRequestException("Dữ liệu phân công đề tài cũ không khớp với danh sách các đội được đi tiếp. Vui lòng kiểm tra lại dữ liệu.");
                }

                return; // Nếu dữ liệu khớp thì bỏ qua không chia lại đề để tránh đè dữ liệu.
            }

            // Lấy danh sách đề tài của vòng thi này.
            var roundTopics = await _uow.GetRepository<Topic>().GetAllAsync(t => t.RoundId == round.Id);
            
            var hasNewTopics = roundTopics.Any();
            // Xáo trộn danh sách đề tài để chia ngẫu nhiên
            var topicQueue = hasNewTopics ? new Queue<Topic>(roundTopics.OrderBy(_ => Random.Shared.Next())) : null;

            var now = DateTime.UtcNow;
            var teamRepo = _uow.GetRepository<Team>();
            var submissionRepo = _uow.GetRepository<Submission>();
            var previousRound = await GetPreviousRoundAsync(round);

            foreach (var team in teams)
            {
                Topic? assignedTopic = null;

                if (hasNewTopics)
                {
                    // Lấy topic từ Queue ra (Round Robin) và nhét lại vào cuối Queue để chia đều cho các đội
                    assignedTopic = topicQueue!.Dequeue();
                    topicQueue.Enqueue(assignedTopic);
                }
                else
                {
                    // Nếu vòng này không có Topic nào mới, dùng lại Topic cũ của vòng trước (nếu có)
                    if (team.TopicId.HasValue)
                    {
                        assignedTopic = await _uow.GetRepository<Topic>().GetFirstOrDefaultAsync(t => t.Id == team.TopicId.Value);
                    }
                }

                if (assignedTopic == null)
                    throw new BadRequestException(ErrorMessages.Round.NoTopicToAssign);

                // Lưu vào lịch sử gán Topic của vòng thi
                await roundTeamRepo.AddAsync(new RoundTeam
                {
                    Id = Guid.NewGuid(),
                    RoundId = round.Id,
                    TeamId = team.Id,
                    TopicId = assignedTopic.Id,
                    AssignedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                // Cập nhật Topic hiện tại của Team
                team.TopicId = assignedTopic.Id;
                team.UpdatedAt = now;
                teamRepo.Update(team);

                // Bê Submission từ vòng thi gần nhất qua vòng này (nếu có)
                var oldSubmission = await submissionRepo
                    .GetAllAsync(s => s.TeamId == team.Id)
                    .ContinueWith(t => t.Result.OrderByDescending(s => s.CreatedAt).FirstOrDefault());

                if (oldSubmission != null)
                {
                        var newSubmission = await submissionRepo.GetFirstOrDefaultAsync(s => s.TeamId == team.Id && s.RoundId == round.Id);
                        if (newSubmission == null)
                        {
                            await submissionRepo.AddAsync(new Submission
                            {
                                Id = Guid.NewGuid(),
                                TeamId = team.Id,
                                RoundId = round.Id,
                                PresentationUrl = oldSubmission.PresentationUrl,
                                CreatedAt = now,
                                IsDisqualified = false
                            });
                        }
                    }
                }
            }

        private async Task<List<Team>> GetTeamsQualifiedForRoundAsync(Round round)
        {
            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            bool isFinalTrack = track != null && 
                                (track.Name.Contains("final", StringComparison.OrdinalIgnoreCase) || 
                                 track.Name.Contains("chung kết", StringComparison.OrdinalIgnoreCase));

            if (isFinalTrack)
            {
                var otherTracks = await _uow.GetRepository<Track>()
                    .GetAllAsync(t => t.EventId == track!.EventId && t.Id != track.Id && !t.IsDeleted);

                var qualifiedTeams = new List<Team>();

                foreach (var otherTrack in otherTracks)
                {
                    var allRoundsInOtherTrack = await _uow.GetRepository<Round>()
                        .GetAllAsync(r => r.TrackId == otherTrack.Id);
                    
                    var lastRound = allRoundsInOtherTrack.OrderByDescending(r => r.OrderIndex).FirstOrDefault();

                    if (lastRound != null)
                    {
                        if (!string.Equals(lastRound.Status, RoundConstants.Status.Closed, StringComparison.OrdinalIgnoreCase))
                            throw new BadRequestException($"Không thể mở vòng Chung kết vì bảng đấu '{otherTrack.Name}' vẫn chưa đóng (Closed).");

                        var advancingRankings = await _uow.GetRepository<Domain.Entities.Ranking>()
                            .GetAllAsync(r => r.RoundId == lastRound.Id && r.IsAdvancing);

                        if (!advancingRankings.Any())
                            throw new BadRequestException($"Không thể mở vòng Chung kết vì bảng đấu '{otherTrack.Name}' chưa có đội nào được chốt đi tiếp.");

                        var advancingTeamIds = advancingRankings.Select(r => r.TeamId).ToHashSet();
                        
                        var teams = await _uow.GetRepository<Team>()
                            .GetAllAsync(t => advancingTeamIds.Contains(t.Id) && t.Status == TeamConstants.Status.Approved && !t.IsDeleted);

                        qualifiedTeams.AddRange(teams);
                    }
                }
                return qualifiedTeams;
            }

            // Luồng thông thường cho các Track vòng loại
            var normalTeams = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.TrackId == round.TrackId
                               && t.Status == TeamConstants.Status.Approved
                               && !t.IsDeleted);

            var previousRound = await GetPreviousRoundAsync(round);
            if (previousRound is null)
                return normalTeams;

            EnsurePreviousRoundClosed(previousRound);

            var normalAdvancingRankings = await _uow.GetRepository<Domain.Entities.Ranking>()
                .GetAllAsync(r => r.RoundId == previousRound.Id && r.IsAdvancing);

            if (!normalAdvancingRankings.Any())
                throw new BadRequestException(ErrorMessages.Round.PreviousRoundRankingRequired);

            var normalAdvancingTeamIds = normalAdvancingRankings.Select(r => r.TeamId).ToHashSet();

            return normalTeams.Where(t => normalAdvancingTeamIds.Contains(t.Id)).ToList();
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
