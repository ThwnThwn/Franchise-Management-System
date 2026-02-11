using FranchiseService.Application.Features.ShiftTemplates.DTOs;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Queries.GetShiftTemplates
{
    /// <summary>
    /// Handler xử lý query lấy danh sách mẫu ca.
    /// Logic: Validate StoreId -> Filter -> Map to DTO.
    /// </summary>
    public class GetShiftTemplatesHandler : IRequestHandler<GetShiftTemplatesQuery, List<ShiftTemplateResponse>>
    {
        private readonly IGenericRepository<ShiftTemplate> _templateRepo;
        private readonly IGenericRepository<Store> _storeRepo;

        public GetShiftTemplatesHandler(
            IGenericRepository<ShiftTemplate> templateRepo,
            IGenericRepository<Store> storeRepo)
        {
            _templateRepo = templateRepo;
            _storeRepo = storeRepo;
        }

        public async Task<List<ShiftTemplateResponse>> Handle(GetShiftTemplatesQuery request, CancellationToken cancellationToken)
        {
            // 1. Validate StoreId (Yêu cầu: "muốn chắc chắn")
            if (request.StoreId.HasValue)
            {
                var store = await _storeRepo.GetByIdAsync(request.StoreId.Value);
                if (store == null)
                {
                    // Throw KeyNotFound để Controller trả về 404
                    throw new KeyNotFoundException($"Không tìm thấy Cửa hàng {request.StoreId}");
                }
            }

            // 2. Truy vấn với filter
            var templates = await _templateRepo.GetAllAsync(
                predicate: t =>
                    // Filter theo storeId (nếu có) -> Lấy Store đó HOẶC Global (null)
                    (!request.StoreId.HasValue || t.StoreId == request.StoreId.Value || t.StoreId == null) &&
                    // Filter theo isActive (nếu có)
                    (!request.IsActive.HasValue || t.IsActive == request.IsActive.Value) &&
                    // Filter theo keyword — tìm trong Code hoặc Name
                    (string.IsNullOrEmpty(request.Keyword)
                        || t.Code.Contains(request.Keyword)
                        || t.Name.Contains(request.Keyword))
            );

            // 3. Map Entity → DTO
            return templates.Select(t => new ShiftTemplateResponse
            {
                Id = t.Id,
                StoreId = t.StoreId,
                Code = t.Code,
                Name = t.Name,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                BreakDurationMinutes = t.BreakDurationMinutes ?? 0,
                IsActive = t.IsActive ?? true,
                CreatedAt = t.CreatedAt
            }).ToList();
        }
    }
}
