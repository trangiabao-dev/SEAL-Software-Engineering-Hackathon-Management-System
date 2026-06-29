using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Event;
using SealHackathon.Application.DTOs.Round;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SealHackathon.Domain.Constants;

namespace SealHackathon.Application.Services.Implementations
{
    // Lớp EventService chịu trách nhiệm xử lý toàn bộ logic liên quan đến Giải đấu (Event)
    // Nó triển khai (implement) giao diện IEventService
    public class EventService : IEventService
    {
        // Khai báo biến _uow (Unit Of Work) để giao tiếp với Database
        // Unit of Work giúp đảm bảo tính toàn vẹn dữ liệu (Transaction) khi làm việc với nhiều bảng
        private readonly IUnitOfWork _uow;
        private readonly IServiceProvider _serviceProvider;

        // Constructor tiêm (inject) IUnitOfWork vào service này
        public EventService(IUnitOfWork uow, IServiceProvider serviceProvider)
        {
            _uow = uow;
            _serviceProvider = serviceProvider;
        }

        // Hàm lấy danh sách TẤT CẢ các giải đấu
        public async Task<ApiResponse<List<EventResponse>>> GetAllEventsAsync()
        {
            // Bước 1: Gọi Database lấy lên tất cả các Event với điều kiện chưa bị xóa mềm (IsDeleted == false)
            var events = await _uow.GetRepository<Event>().GetAllAsync(x => !x.IsDeleted);
            
            // Bước 2: Biến đổi dữ liệu thô từ Database (Entity) sang dữ liệu gọn nhẹ (DTO - EventResponse)
            // Việc này giúp giấu đi các trường nhạy cảm và tối ưu băng thông khi gửi về Frontend
            var response = events.Select(e => new EventResponse
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Status = e.Status,
                IsDeleted = e.IsDeleted
            }).ToList();

            // Bước 3: Đóng gói kết quả vào ApiResponse và trả về thành công
            return ApiResponse<List<EventResponse>>.SuccessResult(response);
        }

        // Hàm lấy chi tiết MỘT giải đấu dựa vào ID
        public async Task<ApiResponse<EventResponse>> GetEventByIdAsync(int id)
        {
            // Bước 1: Tìm kiếm Giải đấu trong DB theo Id truyền vào và chưa bị xóa
            var e = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            
            // Nếu không tìm thấy, lập tức ném ra lỗi 404 (NotFoundException) để ngắt luồng
            if (e == null)
            {
                throw new NotFoundException($"Không tìm thấy Event với ID {id}");
            }

            // Bước 2: Map dữ liệu sang DTO
            var response = new EventResponse
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Status = e.Status,
                IsDeleted = e.IsDeleted
            };

            // Trả về dữ liệu cho Frontend
            return ApiResponse<EventResponse>.SuccessResult(response);
        }

        // Hàm lấy Event đang Active hoặc Registration hiện tại
        public async Task<ApiResponse<EventResponse>> GetActiveEventAsync()
        {
            var currentEvents = await _uow.GetRepository<Event>()
                .GetAllAsync(x => (x.Status == EventConstants.Status.Active || x.Status == EventConstants.Status.Registration) && !x.IsDeleted);

            if (!currentEvents.Any())
                throw new NotFoundException(ErrorMessages.Event.CurrentEventNotFound);

            if (currentEvents.Count > 1)
                throw new ConflictException(ErrorMessages.Event.OnlyOneCurrentEventAllowed);

            var activeEvent = currentEvents.First();

            var response = new EventResponse
            {
                Id = activeEvent.Id,
                Name = activeEvent.Name,
                Description = activeEvent.Description,
                StartDate = activeEvent.StartDate,
                EndDate = activeEvent.EndDate,
                Status = activeEvent.Status,
                IsDeleted = activeEvent.IsDeleted
            };

            return ApiResponse<EventResponse>.SuccessResult(response);
        }

        // Hàm TẠO MỚI một giải đấu
        public async Task<ApiResponse<EventResponse>> CreateEventAsync(CreateEventRequest request)
        {
            var status = EventConstants.Status.Registration;
            await EnsureNoOtherCurrentEventAsync(status);

            // Bước 1: Tạo một thực thể Event mới, đổ dữ liệu từ Request (do Frontend gửi) vào
            var newEvent = new Event
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = status, // Trạng thái mặc định là Registration
                CreatedAt = DateTime.UtcNow, // Gắn thời gian tạo là giờ chuẩn quốc tế
                IsDeleted = false
            };

            // Bước 2: Thêm Event mới vào bộ nhớ đệm của Repository
            await _uow.GetRepository<Event>().AddAsync(newEvent);
            
            // Bước 3: Lưu thay đổi xuống thẳng Database vật lý (Lúc này Event mới chính thức có ID)
            await _uow.SaveChangesAsync();

            // Bước 4: Đóng gói Event vừa tạo sang dạng DTO để trả về cho Frontend hiển thị
            var response = new EventResponse
            {
                Id = newEvent.Id,
                Name = newEvent.Name,
                Description = newEvent.Description,
                StartDate = newEvent.StartDate,
                EndDate = newEvent.EndDate,
                Status = newEvent.Status,
                IsDeleted = newEvent.IsDeleted
            };

            return ApiResponse<EventResponse>.SuccessResult(response, "Tạo Event thành công.");
        }

        public async Task<ApiResponse<FullEventResponse>> CreateFullEventAsync(CreateFullEventRequest request)
        {
            await EnsureNoOtherCurrentEventAsync(EventConstants.Status.Registration);

            // 1. Tạo Event gốc
            var newEvent = new Event
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = EventConstants.Status.Registration,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Tracks = new List<Track>() // Khởi tạo List để add
            };

            // 2. Lặp qua các Track
            foreach (var trackDto in request.Tracks)
            {
                var newTrack = new Track
                {
                    Name = trackDto.Name,
                    Description = trackDto.Description,
                    MaxTeams = trackDto.MaxTeams,
                    MaxMembers = trackDto.MaxMembers,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Rounds = new List<Round>()
                };

                // 3. Lặp qua các Round
                int roundOrder = 1;
                foreach (var roundDto in trackDto.Rounds)
                {
                    var newRound = new Round
                    {
                        Name = roundDto.Name,
                        OrderIndex = roundOrder++,
                        StartTime = roundDto.StartTime,
                        EndTime = roundDto.EndTime,
                        AdvancingSlots = roundDto.AdvancingSlots,
                        Status = RoundConstants.Status.Upcoming,
                        CreatedAt = DateTime.UtcNow,
                        Topics = new List<Topic>()
                    };

                    // 4. Gán 1 Topic chung cho vòng thi đầu tiên (Round 1) của Track
                    if (newRound.OrderIndex == 1)
                    {
                        var newTopic = new Topic
                        {
                            Title = request.Topic.Name,
                            Description = request.Topic.Description,
                            Requirements = request.Topic.Requirements,
                            AttachmentUrl = request.Topic.AttachmentUrl,
                            CreatedAt = DateTime.UtcNow
                        };
                        newRound.Topics.Add(newTopic);
                    }
                    newTrack.Rounds.Add(newRound);
                }
                newEvent.Tracks.Add(newTrack);
            }

            // Gắn Event vào Repository và lưu MỘT LẦN DUY NHẤT để tận dụng Object Graph của EF Core
            // Tự động giải quyết bài toán Transaction và N+1 SaveChanges
            await _uow.GetRepository<Event>().AddAsync(newEvent);
            await _uow.SaveChangesAsync(); 

            // Sau khi SaveChangesAsync, EF Core tự động gán ID cho tất cả Event, Track, Round, Topic.
            // Ánh xạ sang Response.
            var response = new FullEventResponse
            {
                Id = newEvent.Id,
                Name = newEvent.Name,
                Description = newEvent.Description,
                StartDate = newEvent.StartDate,
                EndDate = newEvent.EndDate,
                Status = newEvent.Status,
                IsDeleted = newEvent.IsDeleted,
                Tracks = newEvent.Tracks.Select(tr => new FullTrackResponse
                {
                    Id = tr.Id,
                    EventId = tr.EventId,
                    Name = tr.Name,
                    Description = tr.Description,
                    MaxTeams = tr.MaxTeams,
                    MaxMembers = tr.MaxMembers,
                    CurrentTeamCount = 0,
                    Rounds = tr.Rounds.Select(r => new FullRoundResponse
                    {
                        Id = r.Id,
                        TrackId = r.TrackId,
                        Name = r.Name,
                        OrderIndex = r.OrderIndex,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        AdvancingSlots = r.AdvancingSlots,
                        Status = r.Status,
                        Topics = r.Topics.Select(tp => new FullTopicResponse
                        {
                            Id = tp.Id,
                            RoundId = tp.RoundId,
                            Title = tp.Title,
                            Description = tp.Description,
                            Requirements = tp.Requirements,
                            AttachmentUrl = tp.AttachmentUrl
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return ApiResponse<FullEventResponse>.SuccessResult(response, "Tạo cấu trúc Event thành công.");
        }

        // Hàm SỬA thông tin một giải đấu đã có
        public async Task<ApiResponse<EventResponse>> UpdateEventAsync(int id, UpdateEventRequest request)
        {
            var newStatus = NormalizeEventStatus(request.Status);

            // Bước 1: Tìm xem giải đấu đó có tồn tại không
            var existingEvent = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existingEvent == null)
            {
                throw new NotFoundException($"Không tìm thấy Event với ID {id}");
            }

            var statusOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { EventConstants.Status.Draft, 0 },
                { EventConstants.Status.Registration, 1 },
                { EventConstants.Status.Active, 2 },
                { EventConstants.Status.Completed, 3 }
            };

            if (statusOrder.TryGetValue(existingEvent.Status, out int currentOrder) && 
                statusOrder.TryGetValue(newStatus, out int newOrder))
            {
                if (newOrder < currentOrder)
                {
                    throw new BadRequestException("Không thể lùi trạng thái của giải đấu (Event) về trạng thái trước đó.");
                }
            }

            if (newStatus == EventConstants.Status.Completed)
            {
                // Kiểm tra xem tất cả các Track đã hoàn thành chưa (Có vòng chung kết Closed và có Ranking)
                var tracks = await _uow.GetRepository<Track>().GetAllAsync(t => t.EventId == id && !t.IsDeleted);
                if (tracks.Any())
                {
                    var trackIds = tracks.Select(t => t.Id).ToList();
                    var allRounds = await _uow.GetRepository<Round>().GetAllAsync(r => trackIds.Contains(r.TrackId));
                    var rankingsRepo = _uow.GetRepository<Domain.Entities.Ranking>();

                    foreach (var track in tracks)
                    {
                        var finalRound = allRounds
                            .Where(r => r.TrackId == track.Id && r.AdvancingSlots == null)
                            .OrderByDescending(r => r.OrderIndex)
                            .FirstOrDefault();

                        if (finalRound == null)
                        {
                            throw new BadRequestException($"Track '{track.Name}' chưa có vòng chung kết (Final Round). Không thể kết thúc sự kiện.");
                        }

                        if (finalRound.Status != RoundConstants.Status.Closed)
                        {
                            throw new BadRequestException($"Vòng chung kết của Track '{track.Name}' chưa được đóng (Closed). Vui lòng chốt kết quả và đóng vòng thi trước.");
                        }

                        var rankings = await rankingsRepo.GetAllAsync(r => r.RoundId == finalRound.Id);
                        if (!rankings.Any())
                        {
                            throw new BadRequestException($"Vòng chung kết của Track '{track.Name}' chưa có bảng xếp hạng. Vui lòng tính điểm và xếp hạng trước khi đóng sự kiện.");
                        }
                    }
                }
            }

            await EnsureNoOtherCurrentEventAsync(newStatus, id);

            bool isActivating = (newStatus == EventConstants.Status.Active && existingEvent.Status != EventConstants.Status.Active);
            bool isTimeChanged = existingEvent.StartDate != request.StartDate || existingEvent.EndDate != request.EndDate;

            if (isActivating)
            {
                // Kiểm tra Mentor
                var approvedTeams = await _uow.GetRepository<Team>().GetAllAsync(t => t.Track.EventId == id && t.Status == TeamConstants.Status.Approved && !t.IsDeleted);
                if (approvedTeams.Any(t => t.MentorId == null))
                    throw new BadRequestException("Không thể kích hoạt sự kiện. Tất cả các đội được duyệt phải được gán Mentor.");

                // Kiểm tra Judge cho từng Round
                var tracks = await _uow.GetRepository<Track>().GetAllAsync(t => t.EventId == id && !t.IsDeleted);
                var trackIds = tracks.Select(t => t.Id).ToList();
                var rounds = await _uow.GetRepository<Round>().GetAllAsync(r => trackIds.Contains(r.TrackId));
                var judgeAssigns = await _uow.GetRepository<JudgeAssign>().GetAllAsync(ja => rounds.Select(r => r.Id).Contains(ja.RoundId));
                
                foreach (var round in rounds)
                {
                    if (!judgeAssigns.Any(ja => ja.RoundId == round.Id))
                    {
                        throw new BadRequestException($"Không thể kích hoạt sự kiện. Vòng thi '{round.Name}' chưa có giám khảo nào được phân công.");
                    }
                }
            }

            if (isTimeChanged)
            {
                var notificationService = (INotificationService)_serviceProvider.GetService(typeof(INotificationService))!;
                
                var teams = await _uow.GetRepository<Team>().GetAllAsync(t => t.Track.EventId == id && !t.IsDeleted && t.Status != TeamConstants.Status.Disqualified && t.Status != TeamConstants.Status.Rejected);
                var leaderIds = teams.Select(t => t.LeaderId).Distinct().ToList();

                var eventAccounts = await _uow.GetRepository<EventAccount>().GetAllAsync(ea => ea.EventId == id && ea.Status == EventAccountConstants.Status.Approved);
                var staffIds = eventAccounts.Select(ea => ea.AccountId).Distinct().ToList();

                var allTargetIds = leaderIds.Union(staffIds).ToList();

                foreach (var accountId in allTargetIds)
                {
                    await notificationService.SendNotificationAsync(new Application.DTOs.Notification.CreateNotificationRequest
                    {
                        AccountId = accountId,
                        Title = "Thông báo dời lịch sự kiện",
                        Message = $"Lịch trình của sự kiện '{existingEvent.Name}' đã thay đổi. Vui lòng kiểm tra lại thời gian mới.",
                        Type = "EVENT_POSTPONED"
                    });
                }
            }

            // Bước 2: Đè dữ liệu mới (từ request) lên dữ liệu cũ (trong DB)
            existingEvent.Name = request.Name;
            existingEvent.Description = request.Description;
            existingEvent.StartDate = request.StartDate;
            existingEvent.EndDate = request.EndDate;
            existingEvent.Status = newStatus;
            existingEvent.UpdatedAt = DateTime.UtcNow; // Cập nhật lại thời gian sửa

            // Bước 3: Đánh dấu entity này đã bị sửa đổi trong Repository
            _uow.GetRepository<Event>().Update(existingEvent);
            await _uow.SaveChangesAsync();

            // Đóng gói trả về
            var response = new EventResponse
            {
                Id = existingEvent.Id,
                Name = existingEvent.Name,
                Description = existingEvent.Description,
                StartDate = existingEvent.StartDate,
                EndDate = existingEvent.EndDate,
                Status = existingEvent.Status,
                IsDeleted = existingEvent.IsDeleted
            };

            return ApiResponse<EventResponse>.SuccessResult(response, "Cập nhật Event thành công.");
        }

        // Hàm XÓA giải đấu (Sử dụng kỹ thuật Xóa mềm - Soft Delete)
        public async Task<ApiResponse<bool>> DeleteEventAsync(int id)
        {
            // Bước 1: Tìm giải đấu
            var existingEvent = await _uow.GetRepository<Event>().GetFirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (existingEvent == null)
            {
                throw new NotFoundException($"Không tìm thấy Event với ID {id}");
            }

            // Bước 2: Kỹ thuật Xóa Mềm (Soft delete)
            // Thay vì xóa bay màu khỏi DB bằng lệnh DELETE, ta chỉ cập nhật cờ IsDeleted = true
            // Điều này giúp giữ lại lịch sử dữ liệu và có thể khôi phục nếu lỡ xóa nhầm
            existingEvent.IsDeleted = true;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            _uow.GetRepository<Event>().Update(existingEvent);
            await _uow.SaveChangesAsync();

            // Trả về true báo hiệu thành công
            return ApiResponse<bool>.SuccessResult(true, "Xóa Event thành công.");
        }

        private static string NormalizeEventStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);

            status = status.Trim();

            if (!EventConstants.Status.ValidStatuses.Contains(status))
                throw new BadRequestException(ErrorMessages.Common.InvalidStatus);

            if (string.Equals(status, EventConstants.Status.Draft, StringComparison.OrdinalIgnoreCase))
                return EventConstants.Status.Draft;

            if (string.Equals(status, EventConstants.Status.Registration, StringComparison.OrdinalIgnoreCase))
                return EventConstants.Status.Registration;

            if (string.Equals(status, EventConstants.Status.Active, StringComparison.OrdinalIgnoreCase))
                return EventConstants.Status.Active;

            return EventConstants.Status.Completed;
        }

        private async Task EnsureNoOtherCurrentEventAsync(string status, int? currentEventId = null)
        {
            if (!IsCurrentEventStatus(status))
                return;

            var existingCurrentEvent = await _uow.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => !e.IsDeleted
                                          && (!currentEventId.HasValue || e.Id != currentEventId.Value)
                                          && (e.Status == EventConstants.Status.Registration
                                           || e.Status == EventConstants.Status.Active));

            if (existingCurrentEvent is not null)
                throw new ConflictException(ErrorMessages.Event.OnlyOneCurrentEventAllowed);
        }

        private static bool IsCurrentEventStatus(string status)
        {
            return string.Equals(status, EventConstants.Status.Registration, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, EventConstants.Status.Active, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ApiResponse<FullEventResponse>> CloneEventAsync(int id, CloneEventRequest request)
        {
            var oldEvent = await _uow.GetRepository<Event>()
                .GetFirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (oldEvent is null)
                throw new NotFoundException($"Không tìm thấy Event với ID {id}");

            var eventRepo = _uow.GetRepository<Event>();

            var isNameTaken = await eventRepo
                .GetFirstOrDefaultAsync(e => e.Name == request.NewName && !e.IsDeleted);

            if (isNameTaken is not null)
                throw new ConflictException("Tên giải đấu này đã tồn tại trong hệ thống.");

            // Bắt đầu clone với trạng thái mặc định là Registration 
            var newStatus = EventConstants.Status.Registration;

            var newEvent = new Event
            {
                Name = request.NewName,
                Description = oldEvent.Description,
                StartDate = request.NewStartDate,
                EndDate = request.NewEndDate,
                Status = newStatus,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tracks = new List<Track>()
            };

            // Tính toán độ lệch thời gian (Offset) bằng TimeSpan
            var timeOffset = request.NewStartDate.Subtract(oldEvent.StartDate);

            // Fetch toàn bộ Tracks, Rounds, Criterias của oldEvent MỘT LẦN để tránh N+1 Query
            var tracks = await _uow.GetRepository<Track>()
                .GetAllAsync(t => t.EventId == id && !t.IsDeleted);
            
            var trackIds = tracks.Select(t => t.Id).ToList();
            var oldRoundsList = await _uow.GetRepository<Round>()
                .GetAllAsync(r => trackIds.Contains(r.TrackId));
            
            var roundIds = oldRoundsList.Select(r => r.Id).ToList();
            var oldCriteriasList = await _uow.GetRepository<Criterion>()
                .GetAllAsync(c => roundIds.Contains(c.RoundId));

            foreach (var oldTrack in tracks)
            {
                var newTrack = new Track
                {
                    Name = oldTrack.Name,
                    Description = oldTrack.Description,
                    MaxMembers = oldTrack.MaxMembers,
                    MaxTeams = oldTrack.MaxTeams, // Đã bổ sung clone cả MaxTeams
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Rounds = new List<Round>()
                };

                // Lọc các Round thuộc về Track cũ này
                var oldRounds = oldRoundsList.Where(r => r.TrackId == oldTrack.Id);

                foreach (var oldRound in oldRounds)
                {
                    var newRound = new Round
                    {
                        Name = oldRound.Name,
                        StartTime = oldRound.StartTime.Add(timeOffset),
                        EndTime = oldRound.EndTime.Add(timeOffset),
                        OrderIndex = oldRound.OrderIndex,
                        AdvancingSlots = oldRound.AdvancingSlots,
                        Status = RoundConstants.Status.Upcoming,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Criteria = new List<Criterion>()
                    };

                    // Lọc Criteria thuộc về Round cũ này
                    var oldCriterias = oldCriteriasList.Where(c => c.RoundId == oldRound.Id);

                    foreach (var oldCriteria in oldCriterias)
                    {
                        var newCriteria = new Criterion
                        {
                            Name = oldCriteria.Name,
                            MaxScore = oldCriteria.MaxScore,
                            Weight = oldCriteria.Weight,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        newRound.Criteria.Add(newCriteria);
                    }
                    
                    // LƯU Ý QUAN TRỌNG: KHÔNG CLONE TOPIC!
                    // Đề thi (Topics) sẽ được Ban tổ chức tạo mới sau để đảm bảo tính bảo mật.
                    newTrack.Rounds.Add(newRound);
                }
                newEvent.Tracks.Add(newTrack);
            }

            // Lưu tât cả chỉ bằng MỘT lệnh duy nhất nhờ Object Graph của EF Core
            await eventRepo.AddAsync(newEvent);
            await _uow.SaveChangesAsync();

            var responseDto = new FullEventResponse
            {
                Id = newEvent.Id,
                Name = newEvent.Name,
                Description = newEvent.Description,
                StartDate = newEvent.StartDate,
                EndDate = newEvent.EndDate,
                Status = newEvent.Status,
                IsDeleted = newEvent.IsDeleted,
                Tracks = newEvent.Tracks.Select(tr => new FullTrackResponse
                {
                    Id = tr.Id,
                    EventId = tr.EventId,
                    Name = tr.Name,
                    Description = tr.Description,
                    MaxTeams = tr.MaxTeams,
                    MaxMembers = tr.MaxMembers,
                    CurrentTeamCount = 0,
                    Rounds = tr.Rounds.Select(r => new FullRoundResponse
                    {
                        Id = r.Id,
                        TrackId = r.TrackId,
                        Name = r.Name,
                        OrderIndex = r.OrderIndex,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        AdvancingSlots = r.AdvancingSlots,
                        Status = r.Status,
                        Topics = new List<FullTopicResponse>() // Trống vì tính năng clone không copy topic
                    }).ToList()
                }).ToList()
            };

            return ApiResponse<FullEventResponse>.SuccessResult(
                responseDto,
                "Nhân bản giải đấu thành công! Trạng thái hiện tại: Mở đăng ký (Registration)."
            );
        }
    }
}
