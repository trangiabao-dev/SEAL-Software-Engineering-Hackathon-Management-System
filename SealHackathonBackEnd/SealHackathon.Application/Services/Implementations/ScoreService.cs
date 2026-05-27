using SealHackathon.Application.DTOs.Score;
using SealHackathon.Application.Services.Interfaces;
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
            // Kiểm tra Submission có tồn tại không
            // Lý do: Tránh trả về list rỗng khi submissionId không hợp lệ —
            // client sẽ không biết là ID sai hay submission chưa có điểm.
            // Throw NotFoundException rõ ràng hơn.
            var submission = await _unitOfWork
                .GetRepository<Submission>()
                .GetFirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                throw new NotFoundException("Submission", submissionId);

            // Tạm thời trả về list rỗng — sẽ bổ sung sau
            return new List<ScoreRecordResponse>();
        }
    }
}