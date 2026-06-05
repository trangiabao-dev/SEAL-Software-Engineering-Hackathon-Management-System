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
                    Weight = i.Weight
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
            {
                throw new NotFoundException("CriterionTemplate", id);
            }

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
                    Weight = i.Weight
                }).ToList()
            };

            return ApiResponse<CriterionTemplateResponse>.SuccessResult(response);
        }

        public async Task<ApiResponse<CriterionTemplateResponse>> CreateTemplateAsync(CreateCriterionTemplateRequest request)
        {
            var repo = _uow.GetRepository<CriterionTemplate>();

            // Tính tổng weight xem có bằng 100% không (giả sử chuẩn là 100)
            var totalWeight = request.Items.Sum(i => i.Weight);
            if (totalWeight != 100)
            {
                throw new BadRequestException($"Tổng trọng số (Weight) của biểu mẫu phải bằng 100. Hiện tại đang là {totalWeight}.");
            }

            var newTemplate = new CriterionTemplate
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CriterionTemplateItems = request.Items.Select(i => new CriterionTemplateItem
                {
                    Name = i.Name,
                    Description = i.Description,
                    MaxScore = i.MaxScore,
                    Weight = i.Weight
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
                    Weight = i.Weight
                }).ToList()
            };

            return ApiResponse<CriterionTemplateResponse>.SuccessResult(response, "Tạo biểu mẫu tiêu chí thành công.");
        }

        public async Task<ApiResponse<bool>> DeleteTemplateAsync(int id)
        {
            var repo = _uow.GetRepository<CriterionTemplate>();
            var itemRepo = _uow.GetRepository<CriterionTemplateItem>();
            
            var template = await repo.GetFirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                throw new NotFoundException("CriterionTemplate", id);
            }

            // Xóa các item con trước
            var items = await itemRepo.GetAllAsync(i => i.TemplateId == id);
            foreach(var item in items)
            {
                itemRepo.Delete(item);
            }

            repo.Delete(template);
            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Đã xóa biểu mẫu tiêu chí thành công.");
        }
    }
}
