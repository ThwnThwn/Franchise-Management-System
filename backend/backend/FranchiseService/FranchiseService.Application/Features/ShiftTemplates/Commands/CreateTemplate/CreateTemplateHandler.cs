using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;
using MediatR;

namespace FranchiseService.Application.Features.ShiftTemplates.Commands.CreateTemplate
{
    /// <summary>
    /// Handler tạo mẫu ca làm việc mới.
    /// </summary>
    public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, Guid>
    {
        private readonly IGenericRepository<ShiftTemplate> _templateRepo;
        private readonly IGenericRepository<Store> _storeRepo;

        public CreateTemplateHandler(
            IGenericRepository<ShiftTemplate> templateRepo,
            IGenericRepository<Store> storeRepo)
        {
            _templateRepo = templateRepo;
            _storeRepo = storeRepo;
        }

        public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate StoreId (User requested: "muốn chắc chắn")
            if (request.StoreId.HasValue)
            {
                var store = await _storeRepo.GetByIdAsync(request.StoreId.Value);
                if (store == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy Cửa hàng {request.StoreId}");
                }
            }

            // 2. Validate Duplicate Code in Store
            // Code mẫu ca phải là duy nhất trong cùng 1 Store
            var existingTemplates = await _templateRepo.GetAllAsync(
                predicate: t => t.StoreId == request.StoreId && t.Code == request.Code
            );
            
            if (existingTemplates.Any())
            {
                throw new InvalidOperationException($"Mã mẫu ca '{request.Code}' đã tồn tại trong cửa hàng này.");
            }

            // 3. Create Entity
            var template = new ShiftTemplate
            {
                Id = Guid.NewGuid(),
                StoreId = request.StoreId,
                Code = request.Code,
                Name = request.Name,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                BreakDurationMinutes = request.BreakDurationMinutes,
                IsActive = true, // Mặc định khi tạo mới là active
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _templateRepo.AddAsync(template);

            return template.Id;
        }
    }
}
