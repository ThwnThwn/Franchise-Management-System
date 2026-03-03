using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using SharedKernel.Events;

namespace ProductService.Application.Features.Products.Command.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IProductUow uow,
    IIntegrationEventBus eventBus,
    IMapper mapper
) : IRequestHandler<UpdateProductCommand, Result<UpdateProductCommandResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IProductUow _uow = uow;
    private readonly IIntegrationEventBus _eventBus = eventBus;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<UpdateProductCommandResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra nghiệp vụ (Validate)
        var tmpProd = await _productRepository
            .GetProductByNameAsync(request.Request.Name ?? "", request.ProductId);
        if (tmpProd is not null)
        {
            return Result.Conflict($"Product with name '{request.Request.Name}' already exists");
        }

        var checkCategory = await _categoryRepository.GetByIdAsync(request.Request.CategoryId);
        if (checkCategory is null)
        {
            return Result.NotFound($"Category with id '{request.Request.CategoryId}' not found");
        }

        // 2. Bắt đầu Transaction tích hợp CAP Outbox
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = new Domain.Entities.Product
            {
                Id = request.ProductId,
                Name = request.Request.Name ?? "",
                Description = request.Request.Description,
                IsActive = request.Request.IsActive,
                CategoryId = request.Request.CategoryId,
                ProductVariants = request.Request.Variants?.Select(v => new Domain.Entities.ProductVariant
                {
                    Sku = v.Sku ?? "",
                    Name = v.Name ?? "",
                    Attributes = v.Attributes,
                    BasePrice = v.BasePrice ?? 0,
                    Cost = v.Cost ?? 0,
                    IsActive = v.IsActive ?? true
                }).ToList() ?? new List<Domain.Entities.ProductVariant>()
            };

            // 3. Thực thi logic Update (Chỉ thay đổi State, không gọi SaveChanges)
            var produdctRes = await _productRepository.UpdateWithVariantsOnlyAsync(product);

            // 4. Chuẩn bị Integration Event cho Elasticsearch
            var integrationEvent = new ProductPersistedIntegrationEvent(
                produdctRes.Id,
                produdctRes.Name,
                produdctRes.Description,
                produdctRes.ImageUrl, // Lấy lại từ state nếu cần
                produdctRes.ImagePublicId,
                produdctRes.IsActive ?? true,
                produdctRes.IsDeleted, // IsDeleted = false
                produdctRes.CreatedAt, // CreatedAt (Tạm thời, Subscriber sẽ Upsert)
                produdctRes.UpdatedAt, // UpdatedAt
                produdctRes.CategoryId,
                checkCategory.Name, // Tên danh mục để Search
                produdctRes.ProductVariants.Select(v => new ProductVariantIntegrationModel(
                    v.Id, v.Sku, v.Name, v.Attributes, v.BasePrice, v.Cost, v.IsActive ?? true
                )).ToList()
            );

            // Bắn vào Outbox
            await _eventBus.PublishAsync("product.persisted", integrationEvent, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success(_mapper.Map<UpdateProductCommandResponse>(produdctRes));
        }
        catch (Exception)
        {
            await _uow.RollbackAsync(cancellationToken);
            throw;
        }
    }
}