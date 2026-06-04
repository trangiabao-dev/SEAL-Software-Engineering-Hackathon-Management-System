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
            if (roundExists == null) throw new NotFoundException($"Không tìm thấy Round với ID {roundId}");

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
            if (roundExists == null) throw new NotFoundException($"Không tìm thấy Round với ID {request.RoundId}");

            // Thực thi RULE 5: Tổng trọng số (Weight) không được vượt quá 1.0
            var existingCriteria = await _uow.GetRepository<Criterion>().GetAllAsync(x => x.RoundId == request.RoundId);
            double currentSum = existingCriteria.Sum(x => x.Weight);
            double expectedSum = currentSum + request.Weight;

            // Kiểm tra xem tổng mới có vượt quá 1.0 không
            if (expectedSum > 1.0)
            {
                throw new BadRequestException($"Tổng trọng số vượt quá 1.0 (Hiện tại: {currentSum}, Yêu cầu thêm: {request.Weight}). Vui lòng điều chỉnh!");
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

            return ApiResponse<CriterionResponse>.SuccessResult(response, "Tạo Tiêu chí thành công.");
        }

        public async Task<ApiResponse<bool>> ImportFromTemplateAsync(ImportCriterionRequest request)
        {
            var roundExists = await _uow.GetRepository<Round>().GetFirstOrDefaultAsync(x => x.Id == request.RoundId);
            if (roundExists == null) throw new NotFoundException($"Không tìm thấy Round với ID {request.RoundId}");

            // Lấy các Items từ Template
            var templateItems = await _uow.GetRepository<CriterionTemplateItem>().GetAllAsync(x => x.TemplateId == request.TemplateId);
            if (!templateItems.Any()) throw new BadRequestException($"Template này không có tiêu chí nào để import.");

            // Áp dụng Rule 5 kiểm tra tổng trước khi import
            var existingCriteria = await _uow.GetRepository<Criterion>().GetAllAsync(x => x.RoundId == request.RoundId);
            double currentSum = existingCriteria.Sum(x => x.Weight);
            double importSum = templateItems.Sum(x => x.Weight);

            if (currentSum + importSum > 1.0)
            {
                throw new BadRequestException($"Import thất bại! Tổng trọng số sau khi import sẽ vượt quá 1.0 (Hiện tại: {currentSum}, Import thêm: {importSum}).");
            }

            foreach (var item in templateItems)
            {
                var newCriterion = new Criterion
                {
                    RoundId = request.RoundId,
                    Name = item.Name,
                    Description = item.Description,
                    MaxScore = item.MaxScore,
                    Weight = item.Weight,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.GetRepository<Criterion>().AddAsync(newCriterion);
            }

            await _uow.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, $"Import thành công {templateItems.Count} tiêu chí.");
        }
    }
}
