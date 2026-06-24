using SealHackathon.Application.Common.Exports;
using SealHackathon.Application.Common.Rules;
using SealHackathon.Application.DTOs.Prize;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Constants;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Exceptions;
using SealHackathon.Domain.Interfaces.Repositories;
using PrizeEntity = SealHackathon.Domain.Entities.Prize;
using RankingEntity = SealHackathon.Domain.Entities.Ranking;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Xử lý nghiệp vụ cấu hình giải thưởng và xuất kết quả đạt giải theo Ranking.
    /// </summary>
    public class PrizeService : IPrizeService
    {
        private static readonly int[] AwardRankPositions = { 1, 2, 3 };

        private readonly IUnitOfWork _unitOfWork;

        public PrizeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách cấu hình giải thưởng của một Track.
        /// </summary>
        public async Task<List<PrizeResponse>> GetPrizesByTrackAsync(int trackId)
        {
            await EnsureTrackExistsAsync(trackId);

            var prizes = await _unitOfWork
                .GetRepository<PrizeEntity>()
                .GetAllAsync(p => p.TrackId == trackId);

            return prizes
                .OrderBy(p => p.RankPosition)
                .Select(MapToPrizeResponse)
                .ToList();
        }

        /// <summary>
        /// Tạo cấu hình giải thưởng cho một Track.
        /// </summary>
        public async Task<PrizeResponse> CreatePrizeAsync(int trackId, CreatePrizeRequest request)
        {
            await EnsureTrackExistsAsync(trackId);
            ValidatePrizeInput(request.Name, request.RankPosition, request.Amount);

            // Mỗi Track chỉ được có một giải cho từng hạng 1, 2 hoặc 3.
            await EnsureRankPositionNotDuplicatedAsync(trackId, request.RankPosition);

            var prize = new PrizeEntity
            {
                TrackId = trackId,
                Name = request.Name.Trim(),
                Description = request.Description,
                RankPosition = request.RankPosition,
                Amount = request.Amount
            };

            await _unitOfWork.GetRepository<PrizeEntity>().AddAsync(prize);
            await _unitOfWork.SaveChangesAsync();

            return MapToPrizeResponse(prize);
        }

        /// <summary>
        /// Cập nhật cấu hình giải thưởng.
        /// </summary>
        public async Task<PrizeResponse> UpdatePrizeAsync(int prizeId, UpdatePrizeRequest request)
        {
            var prize = await _unitOfWork
                .GetRepository<PrizeEntity>()
                .GetFirstOrDefaultTrackingAsync(p => p.Id == prizeId);

            if (prize is null)
                throw new NotFoundException(ErrorMessages.Prize.NotFound);

            ValidatePrizeInput(request.Name, request.RankPosition, request.Amount);

            // Khi cập nhật, bỏ qua chính Prize hiện tại để không tự báo trùng hạng.
            await EnsureRankPositionNotDuplicatedAsync(
                prize.TrackId,
                request.RankPosition,
                prize.Id);

            prize.Name = request.Name.Trim();
            prize.Description = request.Description;
            prize.RankPosition = request.RankPosition;
            prize.Amount = request.Amount;

            await _unitOfWork.SaveChangesAsync();

            return MapToPrizeResponse(prize);
        }

        /// <summary>
        /// Xóa cấu hình giải thưởng.
        /// </summary>
        public async Task<bool> DeletePrizeAsync(int prizeId)
        {
            var prize = await _unitOfWork
                .GetRepository<PrizeEntity>()
                .GetFirstOrDefaultTrackingAsync(p => p.Id == prizeId);

            if (prize is null)
                throw new NotFoundException(ErrorMessages.Prize.NotFound);

            _unitOfWork.GetRepository<PrizeEntity>().Delete(prize);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Lấy danh sách đội đạt giải hạng 1, 2, 3 của một Round.
        /// </summary>
        public async Task<List<PrizeWinnerResponse>> GetWinnersByRoundAsync(int roundId)
        {
            var round = await GetRoundOrThrowAsync(roundId);
            var track = await GetTrackOrThrowAsync(round.TrackId);

            await EnsureRoundCanDeterminePrizesAsync(round, track.Id);

            var prizes = await GetConfiguredPrizesOrThrowAsync(track.Id);
            var rankings = await GetPrizeRankingsOrThrowAsync(round.Id);

            EnsureNoTieInPrizeRanks(rankings);
            EnsureRequiredPrizeRanksHaveWinners(rankings);

            var teamIds = rankings.Select(r => r.TeamId).Distinct().ToList();
            var teams = await _unitOfWork
                .GetRepository<Team>()
                .GetAllAsync(t => teamIds.Contains(t.Id));

            var teamDict = teams.ToDictionary(t => t.Id, t => t);
            var rankingDict = rankings.ToDictionary(r => r.RankPosition);

            var result = new List<PrizeWinnerResponse>();
            foreach (var prize in prizes.OrderBy(p => p.RankPosition))
            {
                var ranking = rankingDict[prize.RankPosition];

                if (!teamDict.TryGetValue(ranking.TeamId, out var team))
                    throw new NotFoundException(ErrorMessages.Team.NotFound);

                result.Add(MapToPrizeWinnerResponse(prize, ranking, team, round, track));
            }

            return result;
        }

        /// <summary>
        /// Xuất danh sách đội đạt giải của một Round ra file XLSX.
        /// </summary>
        public async Task<byte[]> ExportWinnersByRoundAsync(int roundId)
        {
            var winners = await GetWinnersByRoundAsync(roundId);
            var headers = new[]
            {
                "PrizeId",
                "PrizeName",
                "PrizeDescription",
                "RankPosition",
                "Amount",
                "TrackId",
                "TrackName",
                "RoundId",
                "RoundName",
                "TeamId",
                "TeamName",
                "University",
                "TotalScore",
                "CalculatedAt"
            };

            // Export theo DTO PrizeWinnerResponse để file Excel khớp dữ liệu FE nhìn thấy.
            var rows = winners
                .Select(winner => new object?[]
                {
                    winner.PrizeId,
                    winner.PrizeName,
                    winner.PrizeDescription,
                    winner.RankPosition,
                    winner.Amount,
                    winner.TrackId,
                    winner.TrackName,
                    winner.RoundId,
                    winner.RoundName,
                    winner.TeamId,
                    winner.TeamName,
                    winner.University,
                    winner.TotalScore,
                    winner.CalculatedAt
                })
                .ToList();

            var numberFormats = new Dictionary<int, string>
            {
                [5] = "#,##0",
                [13] = "0.00",
                [14] = "yyyy-mm-dd hh:mm:ss"
            };

            return ExcelExportHelper.CreateWorkbook(
                "Prize Winners",
                headers,
                rows,
                numberFormats);
        }

        // =============== Private helpers ===============

        /// <summary>
        /// Đảm bảo chỉ Final Round đã đóng mới được xác định và xuất giải thưởng.
        /// </summary>
        private async Task EnsureRoundCanDeterminePrizesAsync(Round round, int trackId)
        {
            var trackRounds = await _unitOfWork
                .GetRepository<Round>()
                .GetAllAsync(item => item.TrackId == trackId);

            var finalRound = FinalRoundRules.GetFinalRound(trackId, trackRounds);

            if (round.Id != finalRound.Id)
                throw new BadRequestException(ErrorMessages.Prize.RoundNotFinal);

            if (!string.Equals(
                    round.Status,
                    RoundConstants.Status.Closed,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.Prize.RoundNotClosed);
            }
        }

        /// <summary>
        /// Kiểm tra Track tồn tại và chưa bị xóa.
        /// </summary>
        private async Task EnsureTrackExistsAsync(int trackId)
        {
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);
        }

        /// <summary>
        /// Lấy Round hoặc báo lỗi nếu Round không tồn tại.
        /// </summary>
        private async Task<Round> GetRoundOrThrowAsync(int roundId)
        {
            var round = await _unitOfWork
                .GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            return round;
        }

        /// <summary>
        /// Lấy Track hoặc báo lỗi nếu Track không tồn tại.
        /// </summary>
        private async Task<Track> GetTrackOrThrowAsync(int trackId)
        {
            var track = await _unitOfWork
                .GetRepository<Track>()
                .GetFirstOrDefaultAsync(t => t.Id == trackId && !t.IsDeleted);

            if (track is null)
                throw new NotFoundException(ErrorMessages.Common.TrackNotFound);

            return track;
        }

        /// <summary>
        /// Kiểm tra dữ liệu cấu hình giải thưởng hợp lệ.
        /// </summary>
        private static void ValidatePrizeInput(string name, int rankPosition, decimal? amount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException(ErrorMessages.Prize.NameRequired);

            if (!AwardRankPositions.Contains(rankPosition))
                throw new BadRequestException(ErrorMessages.Prize.RankPositionInvalid);

            if (amount.HasValue && amount.Value < 0)
                throw new BadRequestException(ErrorMessages.Prize.AmountInvalid);
        }

        /// <summary>
        /// Chặn trùng RankPosition trong cùng Track.
        /// </summary>
        private async Task EnsureRankPositionNotDuplicatedAsync(
            int trackId,
            int rankPosition,
            int? ignoredPrizeId = null)
        {
            var duplicatedPrize = await _unitOfWork
                .GetRepository<PrizeEntity>()
                .GetFirstOrDefaultAsync(p => p.TrackId == trackId
                                             && p.RankPosition == rankPosition
                                             && (!ignoredPrizeId.HasValue || p.Id != ignoredPrizeId.Value));

            if (duplicatedPrize is not null)
                throw new ConflictException(ErrorMessages.Prize.RankPositionDuplicated);
        }

        /// <summary>
        /// Lấy cấu hình Prize hạng 1, 2, 3 của Track.
        /// </summary>
        private async Task<List<PrizeEntity>> GetConfiguredPrizesOrThrowAsync(int trackId)
        {
            var prizes = await _unitOfWork
                .GetRepository<PrizeEntity>()
                .GetAllAsync(p => p.TrackId == trackId
                                  && AwardRankPositions.Contains(p.RankPosition));

            if (!prizes.Any())
                throw new BadRequestException(ErrorMessages.Prize.PrizeConfigNotFound);

            EnsureRequiredPrizeRanksConfigured(prizes);

            return prizes;
        }

        /// <summary>
        /// Đảm bảo Track đã cấu hình đủ giải hạng 1, 2 và 3.
        /// </summary>
        private static void EnsureRequiredPrizeRanksConfigured(List<PrizeEntity> prizes)
        {
            var configuredRanks = prizes.Select(p => p.RankPosition).ToHashSet();

            if (AwardRankPositions.Any(rank => !configuredRanks.Contains(rank)))
                throw new BadRequestException(ErrorMessages.Prize.RequiredRanksNotConfigured);
        }

        /// <summary>
        /// Lấy Ranking hạng 1, 2, 3 để xác định đội đạt giải.
        /// </summary>
        private async Task<List<RankingEntity>> GetPrizeRankingsOrThrowAsync(int roundId)
        {
            var rankings = await _unitOfWork
                .GetRepository<RankingEntity>()
                .GetAllAsync(r => r.RoundId == roundId
                                  && AwardRankPositions.Contains(r.RankPosition)
                                  && !r.Team.IsDeleted
                                  && r.Team.Status != TeamConstants.Status.Disqualified);

            if (!rankings.Any())
                throw new BadRequestException(ErrorMessages.Prize.RoundRankingNotFound);

            return rankings;
        }

        /// <summary>
        /// Chặn export khi ranking còn đồng hạng trong nhóm giải thưởng.
        /// </summary>
        private static void EnsureNoTieInPrizeRanks(List<RankingEntity> rankings)
        {
            // Nếu còn đồng hạng, Coordinator phải tạo round tie-break trước khi xuất giải.
            var hasTie = rankings
                .GroupBy(r => r.RankPosition)
                .Any(g => g.Count() > 1);

            if (hasTie)
                throw new BadRequestException(ErrorMessages.Prize.PrizeRankTieExists);
        }

        /// <summary>
        /// Đảm bảo Ranking có đủ hạng 1, 2 và 3 để map với Prize.
        /// </summary>
        private static void EnsureRequiredPrizeRanksHaveWinners(List<RankingEntity> rankings)
        {
            var rankingRanks = rankings.Select(r => r.RankPosition).ToHashSet();

            if (AwardRankPositions.Any(rank => !rankingRanks.Contains(rank)))
                throw new BadRequestException(ErrorMessages.Prize.PrizeRankWinnerMissing);
        }

        /// <summary>
        /// Chuyển Prize entity sang DTO cấu hình giải thưởng.
        /// </summary>
        private static PrizeResponse MapToPrizeResponse(PrizeEntity prize)
        {
            return new PrizeResponse
            {
                Id = prize.Id,
                TrackId = prize.TrackId,
                Name = prize.Name,
                Description = prize.Description,
                RankPosition = prize.RankPosition,
                Amount = prize.Amount
            };
        }

        /// <summary>
        /// Chuyển dữ liệu Prize, Ranking, Team, Round và Track sang DTO đội đạt giải.
        /// </summary>
        private static PrizeWinnerResponse MapToPrizeWinnerResponse(
            PrizeEntity prize,
            RankingEntity ranking,
            Team team,
            Round round,
            Track track)
        {
            return new PrizeWinnerResponse
            {
                PrizeId = prize.Id,
                PrizeName = prize.Name,
                PrizeDescription = prize.Description,
                RankPosition = prize.RankPosition,
                Amount = prize.Amount,
                TrackId = track.Id,
                TrackName = track.Name,
                RoundId = round.Id,
                RoundName = round.Name,
                TeamId = team.Id,
                TeamName = team.TeamName,
                University = team.University,
                TotalScore = ranking.TotalScore,
                CalculatedAt = ranking.CalculatedAt
            };
        }
    }
}
