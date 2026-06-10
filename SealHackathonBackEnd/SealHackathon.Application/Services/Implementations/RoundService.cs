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
                Status = "Upcoming", // Mới tạo ra thì mặc định là trạng thái "Sắp diễn ra"
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

        // Hàm ĐỔI TRẠNG THÁI Vòng thi (Đặc biệt quan trọng: Có chứa RULE 7)
        public async Task<ApiResponse<RoundResponse>> UpdateRoundStatusAsync(int id, UpdateRoundStatusRequest request)
        {
            var existingRound = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == id);
            if (existingRound == null) throw new NotFoundException($"Không tìm thấy Round với ID {id}");

            existingRound.Status = request.Status;
            existingRound.UpdatedAt = DateTime.UtcNow;

            // ===== RULE 7: TỰ ĐỘNG GÁN ĐỀ TÀI (RANDOM TOPIC ASSIGNMENT) =====
            // Khi Ban Tổ Chức ấn nút Bắt đầu vòng thi (Đổi status sang "Active")
            if (request.Status.Equals(RoundConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
            {
                // Kéo tất cả Đề tài (Topic) của vòng này lên
                var topics = await _uow.GetRepository<Topic>().GetAllAsync(x => x.RoundId == id);
                if (!topics.Any())
                {
                    // Lỗi: Vòng thi làm gì có đề nào mà đòi bắt đầu thi!
                    throw new BadRequestException("Không có Topic nào trong Round này để gán cho các nhóm!");
                }

                // Chỉ team đã được duyệt mới được nhận đề thi.
                var teams = await _uow.GetRepository<Team>().GetAllAsync(x => x.TrackId == existingRound.TrackId
                                                                           && x.Status == TeamConstants.Status.Approved
                                                                           && !x.IsDeleted);

                var teamsWithoutTopic = teams.Where(x => x.TopicId is null).ToList();
                var usedTopicIds = teams.Where(x => x.TopicId.HasValue)
                    .Select(x => x.TopicId!.Value)
                    .ToHashSet();


                // Mỗi team nhận một Topic khác nhau. Team đã có Topic thì không bị đổi lại

                // Mỗi team nhận một Topic khác nhau. Team đã có Topic thì không random lại.

                var random = new Random();
                var availableTopics = topics
                    .Where(x => !usedTopicIds.Contains(x.Id))
                    .OrderBy(_ => random.Next())
                    .ToList();

                if (availableTopics.Count < teamsWithoutTopic.Count)

                    throw new BadRequestException("Số lượng Topic không đủ để gán mỗi đội một đề khác nhau.");


                for (var i = 0; i < teamsWithoutTopic.Count; i++)
                {
                    teamsWithoutTopic[i].TopicId = availableTopics[i].Id;
                    teamsWithoutTopic[i].UpdatedAt = DateTime.UtcNow;
                    _uow.GetRepository<Team>().Update(teamsWithoutTopic[i]); // Lưu tạm sự thay đổi của đội vào bộ nhớ đệm
                }
            }
            // ================================================================

            _uow.GetRepository<Round>().Update(existingRound);
            await _uow.SaveChangesAsync(); // Chạy câu lệnh lưu DB một phát ăn luôn cả Round và Teams

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

            return ApiResponse<RoundResponse>.SuccessResult(response, "Cập nhật trạng thái Round thành công.");
        }

        // Bảo thêm cho Thức sửa lại rule Mentor và Judge
        // Hàm gán GIÁM KHẢO (Judge) vào Vòng thi để họ vào chấm điểm
        public async Task<ApiResponse<bool>> AssignJudgeAsync(int roundId, AssignJudgeRequest request, Guid assignedBy)
        {
            if (roundId <= 0)
                throw new BadRequestException(ErrorMessages.Common.InvalidRoundId);

            if (request.JudgeId == Guid.Empty)
                throw new BadRequestException("JudgeId không hợp lệ.");

            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException("Round", roundId);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", round.TrackId);

            var judge = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == request.JudgeId && !a.IsDeleted);

            if (judge is null)
                throw new NotFoundException("Judge", request.JudgeId);

            var judgeEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == request.JudgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == "Approved");

            if (judgeEventRole is null)
                throw new BadRequestException("Tài khoản này chưa được phân quyền Judge trong Event của Round này.");

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
    }
}
