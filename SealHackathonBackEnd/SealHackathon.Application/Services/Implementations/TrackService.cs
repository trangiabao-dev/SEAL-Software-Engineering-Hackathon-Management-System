using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Track;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Implementations
{
    // Lớp TrackService xử lý các nghiệp vụ liên quan đến Bảng Đấu (Track)
    // Triển khai từ interface ITrackService
    public class TrackService : ITrackService
    {
        // Sử dụng Unit Of Work để quản lý kết nối và các giao dịch DB một cách tập trung
        private readonly IUnitOfWork _uow;

        public TrackService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // Hàm lấy danh sách tất cả các Bảng đấu (Track) thuộc về một Giải đấu (Event) cụ thể
        public async Task<ApiResponse<List<TrackResponse>>> GetTracksByEventIdAsync(int eventId)
        {
            // Bước 1: Kiểm tra xem Event cha có thực sự tồn tại không
            // Nếu gửi ID bậy bạ lên thì chặn ngay lập tức
            var eventExists = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(x => x.Id == eventId && !x.IsDeleted);
            if (eventExists == null) throw new NotFoundException($"Không tìm thấy Event với ID {eventId}");

            // Bước 2: Kéo toàn bộ danh sách Track thỏa mãn điều kiện EventId và chưa bị xóa
            var tracks = await _uow.GetRepository<Track>().GetAllAsync(x => x.EventId == eventId && !x.IsDeleted);
            
            // Bước 3: Đóng gói dữ liệu sang DTO (Data Transfer Object) để Frontend dễ đọc
            var response = tracks.Select(t => new TrackResponse
            {
                Id = t.Id,
                EventId = t.EventId,
                Name = t.Name,
                Description = t.Description,
                MaxTeams = t.MaxTeams, // Số đội tối đa được phép thi ở bảng này
                IsDeleted = t.IsDeleted
            }).ToList();

            return ApiResponse<List<TrackResponse>>.SuccessResult(response);
        }

        // Hàm TẠO MỚI một Bảng đấu (Track)
        public async Task<ApiResponse<TrackResponse>> CreateTrackAsync(CreateTrackRequest request)
        {
            // Bước 1: Kiểm tra tính hợp lệ của Event cha (Giải đấu phải tồn tại mới cho tạo bảng)
            var eventExists = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(x => x.Id == request.EventId && !x.IsDeleted);
            if (eventExists == null) throw new NotFoundException($"Không tìm thấy Event với ID {request.EventId}");

            // Bước 2: Khởi tạo một đối tượng Track mới
            var newTrack = new Track
            {
                EventId = request.EventId, // Nối Track này vào Event
                Name = request.Name,
                Description = request.Description,
                MaxTeams = request.MaxTeams,
                CreatedAt = DateTime.UtcNow, // Lấy thời gian hệ thống
                IsDeleted = false
            };

            // Bước 3: Lưu Track mới vào DB
            await _uow.GetRepository<Track>().AddAsync(newTrack);
            await _uow.SaveChangesAsync(); // Chạy câu lệnh INSERT xuống SQL

            // Bước 4: Chuyển đổi Track vừa tạo sang DTO để trả về phản hồi
            var response = new TrackResponse
            {
                Id = newTrack.Id,
                EventId = newTrack.EventId,
                Name = newTrack.Name,
                Description = newTrack.Description,
                MaxTeams = newTrack.MaxTeams,
                IsDeleted = newTrack.IsDeleted
            };

            return ApiResponse<TrackResponse>.SuccessResult(response, "Tạo Track thành công.");
        }

        // Hàm SỬA Bảng đấu
        public async Task<ApiResponse<TrackResponse>> UpdateTrackAsync(int id, UpdateTrackRequest request)
        {
            // Bước 1: Tìm Track cần sửa dựa vào ID
            var existingTrack = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existingTrack == null) throw new NotFoundException($"Không tìm thấy Track với ID {id}");

            // Bước 2: Cập nhật các trường thông tin mới từ Request
            existingTrack.Name = request.Name;
            existingTrack.Description = request.Description;
            existingTrack.MaxTeams = request.MaxTeams;
            existingTrack.UpdatedAt = DateTime.UtcNow; // Ghi nhận thời điểm bị sửa

            // Bước 3: Ghi nhận sự thay đổi và lưu vào DB
            _uow.GetRepository<Track>().Update(existingTrack);
            await _uow.SaveChangesAsync(); // Chạy câu lệnh UPDATE xuống SQL

            var response = new TrackResponse
            {
                Id = existingTrack.Id,
                EventId = existingTrack.EventId,
                Name = existingTrack.Name,
                Description = existingTrack.Description,
                MaxTeams = existingTrack.MaxTeams,
                IsDeleted = existingTrack.IsDeleted
            };

            return ApiResponse<TrackResponse>.SuccessResult(response, "Cập nhật Track thành công.");
        }

        // Hàm phân công Ban Giám Khảo (Mentor/Giám khảo hỗ trợ) vào một Bảng đấu
        public async Task<ApiResponse<bool>> AssignMentorAsync(int trackId, AssignMentorRequest request, Guid assignedBy)
        {
            // Bước 1: Kiểm tra Bảng đấu có tồn tại không
            var trackExists = await _uow.GetRepository<Track>().GetFirstOrDefaultAsync(x => x.Id == trackId && !x.IsDeleted);
            if (trackExists == null) throw new NotFoundException($"Không tìm thấy Track với ID {trackId}");

            // Bước 2: Tạo bản ghi gán quyền (MentorAssign) kết nối giữa Track và Tài khoản Mentor
            var mentorAssign = new MentorAssign
            {
                TrackId = trackId, // Bảng đấu nào
                MentorId = request.MentorId, // Ai làm Mentor
                AssignedAt = DateTime.UtcNow, // Phân công lúc mấy giờ
                AssignedBy = assignedBy // Ai là người đứng ra phân công (Coordinator ID)
            };

            // Bước 3: Lưu vào DB
            await _uow.GetRepository<MentorAssign>().AddAsync(mentorAssign);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Phân công Mentor vào Bảng đấu thành công.");
        }
    }
}
