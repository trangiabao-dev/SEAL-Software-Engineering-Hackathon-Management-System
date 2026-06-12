using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Criteria;
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
    public class CriterionService : ICriterionService
    {
        private readonly IUnitOfWork _uow;
        private const double WeightTolerance = 0.01;

        public CriterionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<CriterionResponse>>> GetCriteriaByRoundIdAsync(int roundId)
        {
            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == roundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            var criteria = await _uow.GetRepository<Criterion>()
                .GetAllAsync(c => c.RoundId == roundId);

            var response = criteria.Select(MapToResponse).ToList();

            return ApiResponse<List<CriterionResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<CriterionResponse>> CreateCriterionAsync(CreateCriterionRequest request)
        {
            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == request.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            ValidateCriterionInput(request.Name, request.MaxScore, request.Weight);

            var criterionRepo = _uow.GetRepository<Criterion>();

            var existingCriteria = await criterionRepo
                .GetAllAsync(c => c.RoundId == request.RoundId);

            EnsureCriterionNameNotDuplicated(existingCriteria, request.Name);

            var currentTotalWeight = existingCriteria.Sum(c => ToDisplayWeight(c.Weight));
            var expectedTotalWeight = currentTotalWeight + request.Weight;

            if (expectedTotalWeight > 100.0 + WeightTolerance)
                throw new BadRequestException(ErrorMessages.Criterion.WeightTotalExceeded);

            var criterion = new Criterion
            {
                RoundId = request.RoundId,
                Name = request.Name.Trim(),
                Description = request.Description,
                MaxScore = request.MaxScore,
                Weight = ToStoredWeight(request.Weight),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await criterionRepo.AddAsync(criterion);
            await _uow.SaveChangesAsync();

            return ApiResponse<CriterionResponse>.SuccessResult(
                MapToResponse(criterion),
                "Tạo tiêu chí thành công.");
        }

        public async Task<ApiResponse<bool>> ImportFromTemplateAsync(ImportCriterionRequest request)
        {
            var round = await _uow.GetRepository<Round>()
                .GetFirstOrDefaultAsync(r => r.Id == request.RoundId);

            if (round is null)
                throw new NotFoundException(ErrorMessages.Common.RoundNotFound);

            var templateItems = await _uow.GetRepository<CriterionTemplateItem>()
                .GetAllAsync(i => i.TemplateId == request.TemplateId);

            if (!templateItems.Any())
                throw new BadRequestException(ErrorMessages.Criterion.TemplateHasNoCriteria);

            var criterionRepo = _uow.GetRepository<Criterion>();

            var existingCriteria = await criterionRepo
                .GetAllAsync(c => c.RoundId == request.RoundId);

            var duplicatedNames = templateItems
                .Select(i => i.Name)
                .Intersect(existingCriteria.Select(c => c.Name), StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (duplicatedNames.Any())
                throw new ConflictException(ErrorMessages.Criterion.TemplateImportDuplicatedName);

            var currentTotalWeight = existingCriteria.Sum(c => c.Weight);
            var importTotalWeight = templateItems.Sum(i => i.Weight);

            if (currentTotalWeight + importTotalWeight > 1.0 + WeightTolerance)
                throw new BadRequestException(ErrorMessages.Criterion.WeightTotalExceeded);

            foreach (var item in templateItems)
            {
                var criterion = new Criterion
                {
                    RoundId = request.RoundId,
                    Name = item.Name,
                    Description = item.Description,
                    MaxScore = item.MaxScore,
                    Weight = item.Weight,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await criterionRepo.AddAsync(criterion);
            }

            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Import tiêu chí từ template thành công.");
        }

        public async Task<ApiResponse<CriterionResponse>> UpdateCriterionAsync(
            int roundId, int criterionId, UpdateCriterionRequest request)
        {
            var criterionRepo = _uow.GetRepository<Criterion>();

            var criterion = await criterionRepo
                .GetFirstOrDefaultTrackingAsync(c => c.Id == criterionId);

            if (criterion is null)
                throw new NotFoundException(ErrorMessages.Criterion.NotFound);

            if (criterion.RoundId != roundId)
                throw new BadRequestException(ErrorMessages.Criterion.NotBelongToRound);

            await EnsureCriterionNotUsedByScoreAsync(criterionId);

            ValidateCriterionInput(request.Name, request.MaxScore, request.Weight);

            var criteriaInRound = await criterionRepo
                .GetAllAsync(c => c.RoundId == roundId);

            EnsureCriterionNameNotDuplicated(criteriaInRound, request.Name, criterionId);

            var totalWeightWithoutCurrent = criteriaInRound
                .Where(c => c.Id != criterionId)
                .Sum(c => ToDisplayWeight(c.Weight));

            var expectedTotalWeight = totalWeightWithoutCurrent + request.Weight;

            if (expectedTotalWeight > 100.0 + WeightTolerance)
                throw new BadRequestException(ErrorMessages.Criterion.WeightTotalExceeded);

            criterion.Name = request.Name.Trim();
            criterion.Description = request.Description;
            criterion.MaxScore = request.MaxScore;
            criterion.Weight = ToStoredWeight(request.Weight);
            criterion.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync();

            return ApiResponse<CriterionResponse>.SuccessResult(
                MapToResponse(criterion),
                "Cập nhật tiêu chí thành công.");
        }

        public async Task<ApiResponse<bool>> DeleteCriterionAsync(int roundId, int criterionId)
        {
            var criterionRepo = _uow.GetRepository<Criterion>();

            var criterion = await criterionRepo
                .GetFirstOrDefaultTrackingAsync(c => c.Id == criterionId);

            if (criterion is null)
                throw new NotFoundException(ErrorMessages.Criterion.NotFound);

            if (criterion.RoundId != roundId)
                throw new BadRequestException(ErrorMessages.Criterion.NotBelongToRound);

            await EnsureCriterionNotUsedByScoreAsync(criterionId);

            criterionRepo.Delete(criterion);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Xóa tiêu chí thành công.");
        }

        private static void ValidateCriterionInput(string name, double maxScore, double weight)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException(ErrorMessages.Criterion.NameRequired);

            if (maxScore <= 0)
                throw new BadRequestException(ErrorMessages.Criterion.MaxScoreInvalid);

            if (weight <= 0 || weight > 100)
                throw new BadRequestException(ErrorMessages.Criterion.WeightInvalid);
        }

        private static void EnsureCriterionNameNotDuplicated(
            List<Criterion> criteria,
            string name,
            int? ignoreCriterionId = null)
        {
            var duplicated = criteria.Any(c =>
                (!ignoreCriterionId.HasValue || c.Id != ignoreCriterionId.Value) &&
                string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (duplicated)
                throw new ConflictException(ErrorMessages.Criterion.NameDuplicatedInRound);
        }

        private async Task EnsureCriterionNotUsedByScoreAsync(int criterionId)
        {
            var scoreRecord = await _uow.GetRepository<ScoreRecord>()
                .GetFirstOrDefaultAsync(s => s.CriterionId == criterionId);

            if (scoreRecord is not null)
                throw new BadRequestException(ErrorMessages.Criterion.AlreadyUsedByScore);
        }

        private static CriterionResponse MapToResponse(Criterion criterion)
        {
            return new CriterionResponse
            {
                Id = criterion.Id,
                RoundId = criterion.RoundId,
                Name = criterion.Name,
                Description = criterion.Description,
                MaxScore = criterion.MaxScore,
                Weight = ToDisplayWeight(criterion.Weight)
            };
        }

        private static double ToStoredWeight(double displayWeight)
        {
            return displayWeight / 100.0;
        }

        private static double ToDisplayWeight(double storedWeight)
        {
            return Math.Round(storedWeight * 100.0, 2);
        }
    }
}