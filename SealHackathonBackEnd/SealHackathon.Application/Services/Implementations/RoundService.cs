using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Implementations
{
    // Lớp RoundService điều phối tất cả các hoạt động liên quan đến Vòng Thi (Round)
    public class RoundService : IRoundService
    {
        // Vẫn là UnitOfWork huyền thoại giúp kết nối tới DB
        private readonly IUnitOfWork _uow;

        public RoundService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // Hàm lấy ra danh sách các Vòng thi thuộc về một Bảng đấu cụ thể
        public async Task<ApiResponse<List<RoundResponse>>> GetRoundsByTrackIdAsync(int trackId)
        {
            // Kiểm tra Track cha có tồn tại không
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == trackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException($"Không tìm thấy Track với ID {trackId}");

            // Lấy toàn bộ Vòng thi (Ví dụ: Vòng Sơ loại, Vòng Bán kết...) của Track này
            var rounds = await _uow.GetRepository<Round>().GetAllAsync(x => x.TrackId == trackId);

            // Đóng gói sang định dạng DTO gọn nhẹ để phản hồi về cho Frontend
            var response = rounds.Select(r => new RoundResponse
            {
                Id = r.Id,
                TrackId = r.TrackId,
                Name = r.Name,
                OrderIndex = r.OrderIndex, // Thứ tự của vòng thi (1 = vòng đầu tiên)
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                AdvancingSlots = r.AdvancingSlots, // Số lượng đội được lọt vào vòng tiếp theo
                Status = r.Status
            }).ToList();

            return ApiResponse<List<RoundResponse>>.SuccessResult(response);
        }

        // Hàm TẠO MỚI một Vòng thi
        public async Task<ApiResponse<RoundResponse>> CreateRoundAsync(CreateRoundRequest request)
        {
            // Kiểm tra tính hợp lệ của Track cha
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == request.TrackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException($"Không tìm thấy Track với ID {request.TrackId}");

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
            var response = new RoundResponse
            {
                Id = newRound.Id,
                TrackId = newRound.TrackId,
                Name = newRound.Name,
                OrderIndex = newRound.OrderIndex,
                StartTime = newRound.StartTime,
                EndTime = newRound.EndTime,
                AdvancingSlots = newRound.AdvancingSlots,
                Status = newRound.Status
            };

            return ApiResponse<RoundResponse>.SuccessResult(response, "Tạo Round thành công.");
        }

        // Hàm CẬP NHẬT thông tin Vòng thi (Chỉ đổi Tên, Thời gian, Số slot đi tiếp...)
        public async Task<ApiResponse<RoundResponse>> UpdateRoundAsync(int id, UpdateRoundRequest request)
        {
            var existingRound = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == id);
            if (existingRound == null) throw new NotFoundException($"Không tìm thấy Round với ID {id}");

            // Đè thông tin mới
            existingRound.Name = request.Name;
            existingRound.OrderIndex = request.OrderIndex;
            existingRound.StartTime = request.StartTime;
            existingRound.EndTime = request.EndTime;
            existingRound.AdvancingSlots = request.AdvancingSlots;
            existingRound.UpdatedAt = DateTime.UtcNow;

            _uow.GetRepository<Round>().Update(existingRound);
            await _uow.SaveChangesAsync();

            var response = new RoundResponse
            {
                Id = existingRound.Id,
                TrackId = existingRound.TrackId,
                Name = existingRound.Name,
                OrderIndex = existingRound.OrderIndex,
                StartTime = existingRound.StartTime,
                EndTime = existingRound.EndTime,
                AdvancingSlots = existingRound.AdvancingSlots,
                Status = existingRound.Status
            };

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
                throw new NotFoundException($"Không tìm thấy Round với ID {id}");

            // Nếu status mới giống status hiện tại thì không xử lý lại.
            // Quan trọng: tránh trường hợp bấm Active nhiều lần làm random topic lại.
            if (string.Equals(existingRound.Status, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<RoundResponse>.SuccessResult(
                    MapToRoundResponse(existingRound),
                    "Trạng thái Round không thay đổi.");
            }

            // RULE: Chỉ khi chuyển Round sang Active thì mới gán đề cho team.
            // Các trạng thái khác như Scoring, Closed chỉ đổi status, không random topic.
            if (string.Equals(newStatus, RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
            {
                await AssignTopicsForRoundAsync(existingRound);
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
                throw new ConflictException("Judge này đã được phân công vào Round này.");

            var judgeAssign = new JudgeAssign
            {
                JudgeId = request.JudgeId,
                RoundId = roundId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            };

            await _uow.GetRepository<JudgeAssign>().AddAsync(judgeAssign);
            await _uow.SaveChangesAsync();

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
        private async Task AssignTopicsForRoundAsync(Round round)
        {
            // Lấy tất cả Topic thuộc Round này.
            var topics = await _uow.GetRepository<Topic>()
                .GetAllAsync(x => x.RoundId == round.Id);

            if (!topics.Any())
                throw new BadRequestException("Không có Topic nào trong Round này để gán cho các nhóm.");

            // Chỉ team đã được duyệt mới được nhận đề thi.
            var approvedTeams = await _uow.GetRepository<Team>()
                .GetAllAsync(x => x.TrackId == round.TrackId
                               && x.Status == TeamConstants.Status.Approved
                               && !x.IsDeleted);

            // Chỉ gán đề cho team chưa có Topic.
            // Nếu team đã có Topic thì giữ nguyên để tránh đổi đề giữa chừng.
            var teamsWithoutTopic = approvedTeams
                .Where(x => x.TopicId is null)
                .ToList();

            if (teamsWithoutTopic.Count == 0)
                return;

            // Những Topic đã được team khác dùng sẽ không được gán lại.
            var usedTopicIds = approvedTeams
                .Where(x => x.TopicId.HasValue)
                .Select(x => x.TopicId!.Value)
                .ToHashSet();

            // Random danh sách Topic còn trống.
            var availableTopics = topics
                .Where(x => !usedTopicIds.Contains(x.Id))
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            if (availableTopics.Count < teamsWithoutTopic.Count)
                throw new BadRequestException("Số lượng Topic không đủ để gán mỗi đội một đề khác nhau.");

            var now = DateTime.UtcNow;
            var teamRepo = _uow.GetRepository<Team>();

            // Gán mỗi team một Topic khác nhau.
            for (var i = 0; i < teamsWithoutTopic.Count; i++)
            {
                teamsWithoutTopic[i].TopicId = availableTopics[i].Id;
                teamsWithoutTopic[i].UpdatedAt = now;
                teamRepo.Update(teamsWithoutTopic[i]);
            }
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
