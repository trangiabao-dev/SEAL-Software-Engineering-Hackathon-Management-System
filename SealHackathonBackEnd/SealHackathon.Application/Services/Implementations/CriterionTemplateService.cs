using SealHackathon.Domain.Exceptions;
using SealHackathon.Application.Common.Responses;
using SealHackathon.Application.DTOs.CriterionTemplate;
using SealHackathon.Application.Services.Interfaces;
using SealHackathon.Domain.Entities;
using SealHackathon.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SealHackathon.Application.Services.Implementations
{
    /// <summary>
    /// Criterion Template service.
    ///
    /// Weight convention:
    ///   - Frontend sends and receives weights on a 0-100 scale (e.g. 30 = 30%).
    ///   - Backend stores weights on a 0-1 scale (e.g. 0.30) for ranking calculations.
    ///   - All conversions happen here at the service boundary:
    ///       incoming: weight / 100.0  (store)
    ///       outgoing: weight * 100.0  (display)
    /// </summary>
    public class CriterionTemplateService : ICriterionTemplateService
    {
        private readonly IUnitOfWork _uow;

        public CriterionTemplateService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<List<CriterionTemplateResponse>>> GetAllTemplatesAsync()
        {
            var templateRepo = _uow.GetRepository<CriterionTemplate>();
            var itemRepo = _uow.GetRepository<CriterionTemplateItem>();

            var templates = await templateRepo.GetAllAsync(x => true);
            var allItems = await itemRepo.GetAllAsync(x => true);

            var responseList = templates.Select(t => new CriterionTemplateResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                Items = allItems.Where(i => i.TemplateId == t.Id).Select(i => new CriterionTemplateItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    MaxScore = i.MaxScore,
                    // Backend stores 0..1 → display to frontend as 0..100 (percentage)
                    Weight = Math.Round(i.Weight * 100.0, 2)
                }).ToList()
            }).ToList();

            return ApiResponse<List<CriterionTemplateResponse>>.SuccessResult(responseList);
        }

        public async Task<ApiResponse<CriterionTemplateResponse>> GetTemplateByIdAsync(int id)
        {
            var templateRepo = _uow.GetRepository<CriterionTemplate>();
            var itemRepo = _uow.GetRepository<CriterionTemplateItem>();

            var template = await templateRepo.GetFirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                throw new NotFoundException("CriterionTemplate", id);

            var items = await itemRepo.GetAllAsync(i => i.TemplateId == template.Id);

            var response = new CriterionTemplateResponse
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                CreatedAt = template.CreatedAt,
                Items = items.Select(i => new CriterionTemplateItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    MaxScore = i.MaxScore,
                    // Backend stores 0..1 → display to frontend as 0..100 (percentage)
                    Weight = Math.Round(i.Weight * 100.0, 2)
                }).ToList()
            };

            return ApiResponse<CriterionTemplateResponse>.SuccessResult(response);
        }

        public async Task<ApiResponse<CriterionTemplateResponse>> CreateTemplateAsync(CreateCriterionTemplateRequest request)
        {
            var repo = _uow.GetRepository<CriterionTemplate>();

            // Frontend sends weights as 0-100 (percentages).
            // Validate that the total equals 100 before storing.
            var totalWeight = request.Items.Sum(i => i.Weight);
            if (Math.Abs(totalWeight - 100.0) > 0.01)
            {
                throw new BadRequestException(
                    $"Total weight of all criteria must equal 100. Current total: {totalWeight}.");
            }

            var newTemplate = new CriterionTemplate
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                // Convert each frontend weight (0-100) to backend scale (0-1) before storing
                CriterionTemplateItems = request.Items.Select(i => new CriterionTemplateItem
                {
                    Name = i.Name,
                    Description = i.Description,
                    MaxScore = i.MaxScore,
                    Weight = i.Weight / 100.0   // 30 (%) → 0.30
                }).ToList()
            };

            await repo.AddAsync(newTemplate);
            await _uow.SaveChangesAsync();

            var response = new CriterionTemplateResponse
            {
                Id = newTemplate.Id,
                Name = newTemplate.Name,
                Description = newTemplate.Description,
                CreatedAt = newTemplate.CreatedAt,
                Items = newTemplate.CriterionTemplateItems.Select(i => new CriterionTemplateItemResponse
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    MaxScore = i.MaxScore,
                    // Convert back to 0-100 for frontend display
                    Weight = Math.Round(i.Weight * 100.0, 2)
                }).ToList()
            };

            return ApiResponse<CriterionTemplateResponse>.SuccessResult(response, "Criterion template created successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteTemplateAsync(int id)
        {
            var repo = _uow.GetRepository<CriterionTemplate>();
            var itemRepo = _uow.GetRepository<CriterionTemplateItem>();

            var template = await repo.GetFirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                throw new NotFoundException("CriterionTemplate", id);

            // Delete child items first to avoid FK constraint violations
            var items = await itemRepo.GetAllAsync(i => i.TemplateId == id);
            foreach (var item in items)
            {
                itemRepo.Delete(item);
            }

            repo.Delete(template);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Criterion template deleted successfully.");
        }
    }
}
