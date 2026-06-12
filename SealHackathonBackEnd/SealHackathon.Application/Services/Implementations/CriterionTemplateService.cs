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

            ValidateTemplateInput(request.Name, request.Items);

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

        public async Task<ApiResponse<CriterionTemplateResponse>> UpdateTemplateAsync(
            int id, UpdateCriterionTemplateRequest request)
        {
            var templateRepo = _uow.GetRepository<CriterionTemplate>();
            var itemRepo = _uow.GetRepository<CriterionTemplateItem>();

            var template = await templateRepo.GetFirstOrDefaultTrackingAsync(t => t.Id == id);

            if (template is null)
                throw new NotFoundException("CriterionTemplate", id);

            ValidateTemplateInput(request.Name, request.Items);

            template.Name = request.Name.Trim();
            template.Description = request.Description;

            var oldItems = await itemRepo.GetAllAsync(i => i.TemplateId == id);
            foreach (var oldItem in oldItems)
            {
                itemRepo.Delete(oldItem);
            }

            foreach (var item in request.Items)
            {
                await itemRepo.AddAsync(new CriterionTemplateItem
                {
                    TemplateId = id,
                    Name = item.Name.Trim(),
                    Description = item.Description,
                    MaxScore = item.MaxScore,
                    Weight = item.Weight / 100.0
                });
            }

            await _uow.SaveChangesAsync();

            return await GetTemplateByIdAsync(id);
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

        private static void ValidateTemplateInput(string name, List<CriterionTemplateItemRequest> items)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException("Tên template không được để trống.");

            if (items is null || items.Count == 0)
                throw new BadRequestException("Template phải có ít nhất một tiêu chí.");

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                    throw new BadRequestException("Tên tiêu chí không được để trống.");

                if (item.MaxScore <= 0)
                    throw new BadRequestException("Điểm tối đa phải lớn hơn 0.");

                if (item.Weight <= 0 || item.Weight > 100)
                    throw new BadRequestException("Trọng số phải nằm trong khoảng 1 đến 100.");
            }

            var duplicatedName = items
                .GroupBy(i => i.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);

            if (duplicatedName)
                throw new ConflictException("Tên tiêu chí trong template không được trùng nhau.");

            var totalWeight = items.Sum(i => i.Weight);
            if (Math.Abs(totalWeight - 100.0) > 0.01)
                throw new BadRequestException($"Tổng trọng số phải bằng 100. Hiện tại: {totalWeight}.");
        }
    }
}
