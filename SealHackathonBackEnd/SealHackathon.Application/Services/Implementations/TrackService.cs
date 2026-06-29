using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Track;
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
            var tracks = await _uow.GetRepository<Track>().GetAllWithIncludeAsync(x => x.EventId == eventId && !x.IsDeleted, x => x.Teams);
            
            // Bước 3: Đóng gói dữ liệu sang DTO (Data Transfer Object) để Frontend dễ đọc
            var response = tracks.Select(t => new TrackResponse
            {
                Id = t.Id,
                EventId = t.EventId,
                Name = t.Name,
                Description = t.Description,
                MaxTeams = t.MaxTeams, // Số đội tối đa được phép thi ở bảng này
                MaxMembers = t.MaxMembers,
<<<<<<< Updated upstream
                CurrentTeamCount = t.Teams != null ? t.Teams.Count(tm => !tm.IsDeleted) : 0,
=======
                IsFinal = t.IsFinal,
>>>>>>> Stashed changes
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
            await EnsureSingleFinalTrackAsync(request.EventId, request.IsFinal);

            var newTrack = new Track
            {
                EventId = request.EventId, // Nối Track này vào Event
                Name = request.Name,
                Description = request.Description,
                MaxTeams = request.MaxTeams,
                MaxMembers = request.MaxMembers,
                IsFinal = request.IsFinal,
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
                MaxMembers = newTrack.MaxMembers,
<<<<<<< Updated upstream
                CurrentTeamCount = 0,
=======
                IsFinal = newTrack.IsFinal,
>>>>>>> Stashed changes
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
            await EnsureSingleFinalTrackAsync(existingTrack.EventId, request.IsFinal, existingTrack.Id);

            existingTrack.Name = request.Name;
            existingTrack.Description = request.Description;
            existingTrack.MaxTeams = request.MaxTeams;
            existingTrack.MaxMembers = request.MaxMembers;
            existingTrack.IsFinal = request.IsFinal;
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
                MaxMembers = existingTrack.MaxMembers,
<<<<<<< Updated upstream
                CurrentTeamCount = await _uow.GetRepository<Team>().CountAsync(t => t.TrackId == existingTrack.Id && !t.IsDeleted),
=======
                IsFinal = existingTrack.IsFinal,
>>>>>>> Stashed changes
                IsDeleted = existingTrack.IsDeleted
            };

            return ApiResponse<TrackResponse>.SuccessResult(response, "Cập nhật Track thành công.");
        }

        // Hàm phân công Ban Giám Khảo (Mentor/Giám khảo hỗ trợ) vào một Bảng đấu
        public async Task<ApiResponse<bool>> AssignMentorAsync(int trackId, AssignMentorRequest request, Guid assignedBy)
        {
            if (trackId <= 0)
                throw new BadRequestException("TrackId không hợp lệ.");

            if (request.MentorId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidMentorId);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", trackId);

            var mentor = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == request.MentorId && !a.IsDeleted);

            if (mentor is null)
                throw new NotFoundException(ErrorMessages.Common.MentorNotFound);

            var mentorEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == request.MentorId
                                           && ea.EventRole == RoleConstants.Mentor
                                           && ea.Status == EventAccountConstants.Status.Approved);

            if (mentorEventRole is null)
                throw new BadRequestException("Tài khoản này chưa được phân quyền Mentor trong Event của Track này.");

            var existingAssign = await _uow.GetRepository<MentorAssign>()
                .GetFirstOrDefaultAsync(ma => ma.MentorId == request.MentorId
                                           && ma.TrackId == trackId);

            // Bước 1: Kiểm tra Bảng đấu có tồn tại không
            if (existingAssign is not null)
                throw new ConflictException("Mentor này đã được phân công vào Track này.");

            // Bước 2: Tạo bản ghi gán quyền (MentorAssign) kết nối giữa Track và Tài khoản Mentor
            var mentorAssign = new MentorAssign
            {
                TrackId = trackId,// Bảng đấu nào
                MentorId = request.MentorId, // Ai làm Mentor
                AssignedAt = DateTime.UtcNow, // Phân công lúc mấy giờ
                AssignedBy = assignedBy // Ai là người đứng ra phân công (Coordinator ID)
            };

            // Bước 3: Lưu vào DB
            await _uow.GetRepository<MentorAssign>().AddAsync(mentorAssign);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Phân công Mentor vào Track thành công.");
        }

        public async Task<ApiResponse<MentorTeamAssignmentResponse>> AssignMentorToTeamsAsync(
            int trackId, Guid mentorId, AssignMentorTeamsRequest request, Guid assignedBy)
        {
            if (trackId <= 0)
                throw new BadRequestException("TrackId không hợp lệ.");

            if (mentorId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidMentorId);

            var selectedTeamIds = request.TeamIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!selectedTeamIds.Any())
                throw new BadRequestException(ErrorMessages.Team.TeamIdsRequired);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            await CheckMentorCanManageTrackAsync(track, mentorId);

            // Lấy toàn bộ team trong Track rồi lọc bằng C# để tránh lỗi Contains với SQL Server cũ.
            var teamsInTrack = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.TrackId == trackId && !t.IsDeleted);

            var selectedTeams = teamsInTrack
                .Where(t => selectedTeamIds.Contains(t.Id))
                .ToList();

            if (selectedTeams.Count != selectedTeamIds.Count)
                throw new BadRequestException(ErrorMessages.Team.TeamNotInTrack);

            if (selectedTeams.Any(t => t.Status != TeamConstants.Status.Approved))
                throw new BadRequestException(ErrorMessages.Team.OnlyApprovedCanAssignMentor);

            var teamRepo = _uow.GetRepository<Team>();
            var now = DateTime.UtcNow;

            foreach (var team in selectedTeams)
            {
                team.MentorId = mentorId;
                team.UpdatedAt = now;
                team.UpdatedBy = assignedBy;
                teamRepo.Update(team);
            }

            await _uow.SaveChangesAsync();

            return ApiResponse<MentorTeamAssignmentResponse>.SuccessResult(
                MapToMentorTeamAssignmentResponse(trackId, mentorId, selectedTeams),
                "Phân công Mentor phụ trách team thành công.");
        }

        public async Task<ApiResponse<bool>> AutoAssignMentorsAsync(int trackId, Guid assignedBy)
        {
            if (trackId <= 0)
                throw new BadRequestException("TrackId không hợp lệ.");

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            // 1. Lấy danh sách Mentor đã được phân công vào Track này
            var mentorAssigns = await _uow.GetRepository<MentorAssign>()
                .GetAllAsync(ma => ma.TrackId == trackId);

            var mentorIds = mentorAssigns.Select(ma => ma.MentorId).Distinct().ToList();

            if (!mentorIds.Any())
                throw new BadRequestException("Chưa có Mentor nào được phân công vào Track này.");

            // 2. Lấy danh sách Team đã Approved trong Track và chưa có Mentor
            var teamsWithoutMentor = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.TrackId == trackId 
                               && t.Status == TeamConstants.Status.Approved 
                               && t.MentorId == null 
                               && !t.IsDeleted);

            if (!teamsWithoutMentor.Any())
                throw new BadRequestException("Không có Team hợp lệ nào cần phân công Mentor (các team có thể đã có Mentor hoặc chưa được duyệt).");

            // 3. Phân chia vòng lặp (Round Robin)
            int mentorIndex = 0;
            var now = DateTime.UtcNow;

            foreach (var team in teamsWithoutMentor)
            {
                var mentorId = mentorIds[mentorIndex];
                
                team.MentorId = mentorId;
                team.UpdatedAt = now;
                team.UpdatedBy = assignedBy;
                
                _uow.GetRepository<Team>().Update(team);

                mentorIndex++;
                if (mentorIndex >= mentorIds.Count)
                {
                    mentorIndex = 0;
                }
            }

            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, $"Đã tự động phân công Mentor cho {teamsWithoutMentor.Count} team thành công.");
        }

        public async Task<ApiResponse<MentorTeamAssignmentResponse>> GetMentorTeamsAsync(int trackId, Guid mentorId)
        {
            if (trackId <= 0)
                throw new BadRequestException("TrackId không hợp lệ.");

            if (mentorId == Guid.Empty)
                throw new BadRequestException(ErrorMessages.Common.InvalidMentorId);

            var track = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            await CheckMentorCanManageTrackAsync(track, mentorId);

            var teams = await _uow.GetRepository<Team>()
                .GetAllAsync(t => t.TrackId == trackId
                               && t.MentorId == mentorId
                               && !t.IsDeleted);

            return ApiResponse<MentorTeamAssignmentResponse>.SuccessResult(
                MapToMentorTeamAssignmentResponse(trackId, mentorId, teams),
                "Lấy danh sách team Mentor phụ trách thành công.");
        }

        public async Task<ApiResponse<List<TrackRoundsResponse>>> GetAllTracksWithRoundsAsync()
        {
            var trackRepo = _uow.GetRepository<Track>();
            var roundRepo = _uow.GetRepository<Round>();
            var eventRepo = _uow.GetRepository<Event>();

            var tracks = await trackRepo.GetAllAsync(t => !t.IsDeleted);
            var trackIds = tracks.Select(t => t.Id).ToList();
            
            var events = await eventRepo.GetAllAsync(e => !e.IsDeleted);

            // Bị lỗi SQL Server cũ không hỗ trợ hàm Contains của EF Core 8 (lỗi từ khóa WITH)
            // Khắc phục: Kéo toàn bộ Round lên RAM (do số lượng ít) rồi lọc bằng code C#
            var allRounds = await roundRepo.GetAllAsync(r => true);
            var rounds = allRounds.Where(r => trackIds.Contains(r.TrackId)).ToList();

            var responseList = new List<TrackRoundsResponse>();

            foreach (var track in tracks)
            {
                var ev = events.FirstOrDefault(e => e.Id == track.EventId);
                
                var trackRounds = rounds.Where(r => r.TrackId == track.Id)
                    .OrderBy(r => r.StartTime)
                    .Select(r => new RoundTimelineDto
                    {
                        RoundId = r.Id,
                        Name = r.Name,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        Status = r.Status,
                        AdvancingSlots = r.AdvancingSlots,
                        ProgressPercentage = CalculateProgress(r.StartTime, r.EndTime)
                    }).ToList();

                responseList.Add(new TrackRoundsResponse
                {
                    TrackId = track.Id,
                    TrackName = track.Name,
                    IsFinal = track.IsFinal,
                    EventName = ev?.Name ?? "Unknown Event",
                    Rounds = trackRounds
                });
            }

            return ApiResponse<List<TrackRoundsResponse>>.SuccessResult(responseList);
        }

        private int CalculateProgress(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.UtcNow;
            if (now < startTime) return 0;
            if (now > endTime) return 100;
            
            var totalDuration = (endTime - startTime).TotalMinutes;
            var elapsedDuration = (now - startTime).TotalMinutes;
            
            if (totalDuration <= 0) return 100;
            
            return (int)((elapsedDuration / totalDuration) * 100);
        }

        /// <summary>
        /// Đảm bảo một Event chỉ có tối đa một Track Final đang hoạt động.
        /// </summary>
        private async Task EnsureSingleFinalTrackAsync(int eventId, bool isFinal, int? currentTrackId = null)
        {
            if (!isFinal)
                return;

            // Check ở service giúp FE nhận lỗi rõ ràng; unique index trong database là lớp bảo vệ cuối cùng.
            var existingFinalTrack = await _uow.GetRepository<Track>()
                .GetFirstOrDefaultAsync(track => track.EventId == eventId
                                                 && track.IsFinal
                                                 && !track.IsDeleted
                                                 && (!currentTrackId.HasValue || track.Id != currentTrackId.Value));

            if (existingFinalTrack is not null)
                throw new BadRequestException(ErrorMessages.Track.OnlyOneFinalTrackAllowed);
        }

        private async Task CheckMentorCanManageTrackAsync(Track track, Guid mentorId)
        {
            var mentor = await _uow.GetRepository<Account>()
                .GetFirstOrDefaultAsync(a => a.Id == mentorId && !a.IsDeleted);

            if (mentor is null)
                throw new NotFoundException(ErrorMessages.Common.MentorNotFound);

            var mentorEventRole = await _uow.GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == mentorId
                                           && ea.EventRole == RoleConstants.Mentor
                                           && ea.Status == EventAccountConstants.Status.Approved);

            if (mentorEventRole is null)
                throw new BadRequestException(ErrorMessages.Team.MentorNotInEvent);

            var mentorAssign = await _uow.GetRepository<MentorAssign>()
                .GetFirstOrDefaultAsync(ma => ma.MentorId == mentorId
                                           && ma.TrackId == track.Id);

            if (mentorAssign is null)
                throw new BadRequestException(ErrorMessages.Team.MentorNotAssignedToTrack);
        }

        private static MentorTeamAssignmentResponse MapToMentorTeamAssignmentResponse(
            int trackId, Guid mentorId, List<Team> teams)
        {
            return new MentorTeamAssignmentResponse
            {
                TrackId = trackId,
                MentorId = mentorId,
                AssignedTeams = teams
                    .OrderBy(t => t.TeamName)
                    .Select(t => new MentorAssignedTeamDto
                    {
                        TeamId = t.Id,
                        TeamName = t.TeamName
                    })
                    .ToList()
            };
        }
    }
}
