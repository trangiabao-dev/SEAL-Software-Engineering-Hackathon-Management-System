using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Topic;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    // Implementation của Topic Service
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _uow;

        public TopicService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<TopicResponse>>> GetTopicsByRoundIdAsync(int roundId)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == roundId);
            if (roundExists == null) throw new NotFoundException($"Không tìm thấy Round với ID {roundId}");

            var topics = await _uow.GetRepository<Topic>().GetAllAsync(x => x.RoundId == roundId);
            
            // Đã gỡ bỏ Logic Fallback tự động lấy Đề chung ở đây theo yêu cầu của FE
            // FE sẽ nhận mảng rỗng [] nếu chưa có Topic riêng, từ đó hiển thị Popup cảnh báo

            var response = topics.Select(t => new TopicResponse
            {
                Id = t.Id,
                RoundId = t.RoundId,
                Title = t.Title,
                Description = t.Description,
                Requirements = t.Requirements,
                AttachmentUrl = t.AttachmentUrl
            }).ToList();

            return ApiResponse<List<TopicResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<TopicResponse>> CreateTopicAsync(CreateTopicRequest request)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == request.RoundId);
            if (roundExists == null) throw new NotFoundException($"Không tìm thấy Round với ID {request.RoundId}");

            var newTopic = new Topic
            {
                RoundId = request.RoundId,
                Title = request.Title,
                Description = request.Description,
                Requirements = request.Requirements,
                AttachmentUrl = request.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GetRepository<Topic>().AddAsync(newTopic);
            await _uow.SaveChangesAsync();

            var response = new TopicResponse
            {
                Id = newTopic.Id,
                RoundId = newTopic.RoundId,
                Title = newTopic.Title,
                Description = newTopic.Description,
                Requirements = newTopic.Requirements,
                AttachmentUrl = newTopic.AttachmentUrl
            };

            return ApiResponse<TopicResponse>.SuccessResult(response, "Tạo Topic thành công.");
        }

        public async Task<ApiResponse<TopicResponse>> UpdateTopicAsync(int id, UpdateTopicRequest request)
        {
            var existingTopic = await _uow.GetRepository<Topic>().GetFirstOrDefaultTrackingAsync(x => x.Id == id);
            if (existingTopic == null) throw new NotFoundException($"Không tìm thấy Topic với ID {id}");

            existingTopic.Title = request.Title;
            existingTopic.Description = request.Description;
            existingTopic.Requirements = request.Requirements;
            existingTopic.AttachmentUrl = request.AttachmentUrl;
            existingTopic.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            var response = new TopicResponse
            {
                Id = existingTopic.Id,
                RoundId = existingTopic.RoundId,
                Title = existingTopic.Title,
                Description = existingTopic.Description,
                Requirements = existingTopic.Requirements,
                AttachmentUrl = existingTopic.AttachmentUrl
            };

            return ApiResponse<TopicResponse>.SuccessResult(response, "Cập nhật Topic thành công.");
        }

        public async Task<ApiResponse<bool>> DeleteTopicAsync(int id)
        {
            var existingTopic = await _uow.GetRepository<Topic>().GetFirstOrDefaultTrackingAsync(x => x.Id == id);
            if (existingTopic == null) throw new NotFoundException($"Không tìm thấy Topic với ID {id}");

            // Chỉ xóa mềm nếu DB có hỗ trợ IsDeleted, nhưng hiện tại bảng Topic không có IsDeleted
            _uow.GetRepository<Topic>().Delete(existingTopic);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Xóa Topic thành công.");
        }
    }
}
