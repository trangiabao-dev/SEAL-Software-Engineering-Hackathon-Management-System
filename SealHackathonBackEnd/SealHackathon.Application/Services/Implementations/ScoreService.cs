using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;

namespace SealHackathon.Application.Services.Implementations
{
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScoreRecordResponse> SubmitScoreAsync(
            Guid submissionId,
            Guid judgeId,
            SubmitScoreRequest request)
        {
            // Bước 1: Kiểm tra Submission có tồn tại không
            // Lý do: Client gửi lên submissionId từ URL — nhưng không đảm bảo
            // ID đó thật sự tồn tại trong DB. Nếu không kiểm tra, server sẽ
            // cố lưu ScoreRecord với SubmissionId không hợp lệ → lỗi FK constraint
            // ở tầng DB, rất khó debug. Kiểm tra sớm → throw lỗi rõ ràng hơn.
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException("Submission", submissionId);

            // Bước 2: Kiểm tra Criterion có tồn tại không
            // Lý do: Tương tự Submission — CriterionId do client gửi lên,
            // cần xác nhận tiêu chí đó thật sự thuộc về Round đang diễn ra.
            // Ngoài ra cần lấy object Criterion để dùng MaxScore ở Bước 3
            // và CriterionName ở Bước 5 — không thể bỏ qua bước này.
            var criterion = await _unitOfWork
                .GetRepository<Criterion>()
                .GetFirstOrDefaultAsync(c => c.Id == request.CriterionId);

            if (criterion == null)
                throw new NotFoundException("Criterion", request.CriterionId);

            if (submission.IsDisqualified)
                throw new BadRequestException("Submission này đã bị loại, không thể chấm điểm.");

            if (criterion.RoundId != submission.RoundId)
                throw new BadRequestException("Criterion không thuộc Round của Submission này.");

            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == submission.RoundId);

            if (round is null)
                throw new NotFoundException("Round", submission.RoundId);

            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == round.TrackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException("Track", round.TrackId);

            var activeJudgeInEvent = await _unitOfWork
                .GetRepository<EventAccount>()
                .GetFirstOrDefaultAsync(ea => ea.EventId == track.EventId
                                           && ea.AccountId == judgeId
                                           && ea.EventRole == RoleConstants.Judge
                                           && ea.Status == "Approved"
                                           && !ea.Event.IsDeleted
                                           && ea.Event.Status == "Active");

            if (activeJudgeInEvent is null)
                throw new ForbiddenException("Tài khoản Judge này không còn hoạt động trong Event của vòng thi.");

            var judgeAssign = await _unitOfWork
                .GetRepository<JudgeAssign>()
                .GetFirstOrDefaultAsync(ja => ja.JudgeId == judgeId
                                           && ja.RoundId == submission.RoundId);

            if (judgeAssign is null)
                throw new ForbiddenException("Bạn không được phân công chấm vòng thi này.");

            var existingScore = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(sr => sr.SubmissionId == submissionId
                                           && sr.JudgeId == judgeId
                                           && sr.CriterionId == request.CriterionId);

            if (existingScore is not null)
                throw new ConflictException("Bạn đã chấm tiêu chí này cho bài nộp này rồi.");

            // Bước 3: Kiểm tra điểm có hợp lệ không
            // Lý do: Mỗi tiêu chí có MaxScore riêng — ví dụ tiêu chí "Trình bày"
            // chỉ được chấm tối đa 10 điểm. Nếu Judge nhập 15 hoặc số âm,
            // dữ liệu sẽ bị sai lệch và ảnh hưởng đến Ranking sau này.
            // Validate ở Service thay vì chỉ dựa vào DB constraint —
            // để có thể trả về message lỗi rõ ràng cho Judge.
            if (request.Score < 0 || request.Score > criterion.MaxScore)
                throw new BadRequestException(
                    $"Điểm phải nằm trong khoảng 0 đến {criterion.MaxScore}.");

            // Bước 4: Tạo ScoreRecord mới và lưu vào DB
            // Lý do: Sau khi validate xong xuôi mới tạo entity —
            // đảm bảo dữ liệu lưu vào DB luôn hợp lệ, không có
            // record rác. JudgeId lấy từ JWT token (do Controller truyền xuống)
            // — không lấy từ request body để tránh giả mạo.
            // ScoredAt dùng DateTime.UtcNow — server tự tạo, không để
            // client tự nhập để tránh gian lận thời gian.
            var scoreRecord = new ScoreRecord
            {
                Id = Guid.NewGuid(),
                SubmissionId = submissionId,
                JudgeId = judgeId,
                CriterionId = request.CriterionId,
                Score = request.Score,
                Comment = request.Comment,
                IsCalibration = request.IsCalibration,
                ScoredAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<ScoreRecord>().AddAsync(scoreRecord);
            await _unitOfWork.SaveChangesAsync();

            // Bước 5: Map entity sang Response DTO và trả về
            // Lý do: Không trả thẳng entity ra ngoài — entity chứa
            // navigation properties có thể gây vòng lặp khi serialize JSON.
            // DTO chỉ chứa đúng thông tin client cần — gọn, an toàn, rõ ràng.
            return new ScoreRecordResponse
            {
                Id = scoreRecord.Id,
                SubmissionId = scoreRecord.SubmissionId,
                JudgeId = scoreRecord.JudgeId,
                JudgeName = string.Empty, // Bổ sung sau khi có Account query
                CriterionId = scoreRecord.CriterionId,
                CriterionName = criterion.Name,
                Score = scoreRecord.Score,
                Comment = scoreRecord.Comment,
                IsCalibration = scoreRecord.IsCalibration,
                ScoredAt = scoreRecord.ScoredAt
            };
        }

        public async Task<List<ScoreRecordResponse>> GetScoresBySubmissionAsync(Guid submissionId)
        {
            // Bước 1: Kiểm tra Submission có tồn tại không
            // Lý do: Tránh trả về list rỗng khi submissionId không hợp lệ
            // Client không biết là ID sai hay submission chưa có điểm
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException("Submission", submissionId);

            // Bước 2: Lấy tất cả ScoreRecord của submission này
            // Lý do: Lấy hết điểm của mọi Judge, kể cả IsCalibration
            var scoreRecords = await _unitOfWork
                .GetRepository<ScoreRecord>()
                .GetAllAsync(sr => sr.SubmissionId == submissionId);

            // Nếu chưa có điểm nào thì trả về list rỗng — đây là trường hợp hợp lệ
            // Khác với submissionId không tồn tại ở trên
            if (!scoreRecords.Any())
                return new List<ScoreRecordResponse>();

            // Bước 3: Lấy thông tin Judge và Criterion
            // Lý do: ScoreRecord chỉ lưu JudgeId và CriterionId
            // Cần query thêm để lấy tên — tránh trả về ID thô cho FE

            // Lấy danh sách JudgeId và CriterionId không trùng lặp
            // Lý do dùng Distinct(): 1 Judge có thể chấm nhiều tiêu chí
            // không cần query Account cùng 1 JudgeId nhiều lần
            var judgeIds = scoreRecords.Select(sr => sr.JudgeId).Distinct().ToList();
            var criterionIds = scoreRecords.Select(sr => sr.CriterionId).Distinct().ToList();

            // Query Account cho tất cả JudgeId cùng lúc — 1 round trip duy nhất
            var judges = await _unitOfWork
                .GetRepository<Account>()
                .GetAllAsync(a => judgeIds.Contains(a.Id));

            // Query Criterion cho tất cả CriterionId cùng lúc — 1 round trip duy nhất
            var criteria = await _unitOfWork
                .GetRepository<Criterion>()
                .GetAllAsync(c => criterionIds.Contains(c.Id));

            // Bước 4: Convert sang Dictionary để lookup nhanh O(1)
            // Lý do: Nếu dùng .FirstOrDefault() trong vòng lặp bên dưới
            // thì mỗi iteration lại duyệt toàn bộ list — O(n²) không cần thiết
            var judgeDict = judges.ToDictionary(a => a.Id, a => a.Username);
            var criterionDict = criteria.ToDictionary(c => c.Id, c => c.Name);

            // Bước 5: Map sang Response DTO
            var result = scoreRecords.Select(sr => new ScoreRecordResponse
            {
                Id = sr.Id,
                SubmissionId = sr.SubmissionId,
                JudgeId = sr.JudgeId,
                // GetValueOrDefault: nếu JudgeId không có trong dict
                // trả về string.Empty thay vì throw exception
                JudgeName = judgeDict.GetValueOrDefault(sr.JudgeId, string.Empty),
                CriterionId = sr.CriterionId,
                CriterionName = criterionDict.GetValueOrDefault(sr.CriterionId, string.Empty),
                Score = sr.Score,
                Comment = sr.Comment,
                IsCalibration = sr.IsCalibration,
                ScoredAt = sr.ScoredAt
            }).ToList();

            return result;
        }
    }
}
