using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.Criterion;
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
    // Implementation của Criterion Service
    public class CriterionService : ICriterionService
    {
        private readonly IUnitOfWork _uow;

        public CriterionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<CriterionResponse>>> GetCriteriaByRoundIdAsync(int roundId)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == roundId);
            if (roundExists == null)
                throw new NotFoundException($"Round with ID {roundId} was not found.");

            var criteria = await _uow.GetRepository<Criterion>().GetAllAsync(x => x.RoundId == roundId);
            var response = criteria.Select(c => new CriterionResponse
            {
                Id = c.Id,
                RoundId = c.RoundId,
                Name = c.Name,
                Description = c.Description,
                MaxScore = c.MaxScore,
                Weight = c.Weight
            }).ToList();

            return ApiResponse<List<CriterionResponse>>.SuccessResult(response);
        }

        public async Task<ApiResponse<CriterionResponse>> CreateCriterionAsync(CreateCriterionRequest request)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == request.RoundId);
            if (roundExists == null)
                throw new NotFoundException($"Round with ID {request.RoundId} was not found.");

            // Thực thi RULE 5: Tổng trọng số (Weight) không được vượt quá 1.0
            var existingCriteria = await _uow.GetRepository<Criterion>().GetAllAsync(x => x.RoundId == request.RoundId);
            double currentSum = existingCriteria.Sum(x => x.Weight);
            double expectedSum = currentSum + request.Weight;

            // Kiểm tra xem tổng mới có vượt quá 1.0 không
            if (expectedSum > 1.0)
            {
                throw new BadRequestException(
                    $"Total weight exceeds 1.0 (current: {currentSum:F2}, adding: {request.Weight:F2}). " +
                    "Please adjust the weight value.");
            }

            var newCriterion = new Criterion
            {
                RoundId = request.RoundId,
                Name = request.Name,
                Description = request.Description,
                MaxScore = request.MaxScore,
                Weight = request.Weight,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.GetRepository<Criterion>().AddAsync(newCriterion);
            await _uow.SaveChangesAsync();

            var response = new CriterionResponse
            {
                Id = newCriterion.Id,
                RoundId = newCriterion.RoundId,
                Name = newCriterion.Name,
                Description = newCriterion.Description,
                MaxScore = newCriterion.MaxScore,
                Weight = newCriterion.Weight
            };

            return ApiResponse<CriterionResponse>.SuccessResult(response, "Criterion created successfully.");
        }

        public async Task<ApiResponse<bool>> ImportFromTemplateAsync(ImportCriterionRequest request)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == request.RoundId);
            if (roundExists == null)
                throw new NotFoundException($"Round with ID {request.RoundId} was not found.");

            var templateItems = await _uow.GetRepository<CriterionTemplateItem>().GetAllAsync(x => x.TemplateId == request.TemplateId);
            if (!templateItems.Any())
                throw new BadRequestException("This template has no criteria to import.");

            // Template stores weight on the same 0..1 scale as Criterion.
            // No conversion needed — use the stored values directly.
            var existingCriteria = await _uow.GetRepository<Criterion>().GetAllAsync(x => x.RoundId == request.RoundId);
            double currentSum = existingCriteria.Sum(x => x.Weight);
            double importSum = templateItems.Sum(i => i.Weight);

            if (currentSum + importSum > 1.0)
            {
                throw new BadRequestException(
                    $"Import failed. Total weight after import would exceed 1.0 " +
                    $"(current: {currentSum:F2}, importing: {importSum:F2}).");
            }

            foreach (var item in templateItems)
            {
                var newCriterion = new Criterion
                {
                    RoundId = request.RoundId,
                    Name = item.Name,
                    Description = item.Description,
                    MaxScore = item.MaxScore,
                    Weight = item.Weight,   // Already 0..1, same scale as Criterion
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.GetRepository<Criterion>().AddAsync(newCriterion);
            }

            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true,
                $"Successfully imported {templateItems.Count} criteria from template.");
        }
    }
}
