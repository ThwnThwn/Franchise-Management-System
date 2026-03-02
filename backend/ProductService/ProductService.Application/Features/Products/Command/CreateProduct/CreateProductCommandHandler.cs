using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;
using ProductService.Domain.Entities;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;
using ProductService.Application.Interfaces;
using ProductService.Application.SearchEngines;
using SharedKernel.Events;

namespace ProductService.Application.Features.Products.Command.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IProductUow uow,
    IIntegrationEventBus eventBus,
    IMapper mapper,
    ICloudinaryService cloudinaryService)
        : IRequestHandler<CreateProductCommand, Result<CreateProductCommandResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    private readonly IProductUow _uow = uow;
    private readonly IIntegrationEventBus _eventBus = eventBus;

    private readonly IMapper _mapper = mapper;
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

    /// <summary>
    /// Handle the creation of a new product.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    public async Task<Result<CreateProductCommandResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var exist = await _productRepository.GetProductByNameAsync(request.Name, null);
        if (exist != null)
        {
            return Result.Conflict(
                $"Product with name '{request.Name}' already exists.");
        }

        // Bắt đầu Transaction
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            var product = _mapper.Map<Product>(request);

            // 3. Upload ảnh
            if (request.ImageFile != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(request.ImageFile);
                if (uploadResult != null)
                {
                    product.ImageUrl = uploadResult.Url;
                    product.ImagePublicId = uploadResult.PublicId;
                }
            }

            // 4. Lưu vào DB (Memory state)
            await _productRepository.AddOnlyAsync(product);

            // Lấy thông tin Category để bắn Event (Elastic cần CategoryName)
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            // 5. Bắn Integration Event qua CAP
            var integrationEvent = new ProductPersistedIntegrationEvent(
                product.Id,
                product.Name,
                product.Description,
                product.ImageUrl,
                product.ImagePublicId,
                product.IsActive ?? true,
                product.IsDeleted,
                product.CreatedAt,
                product.UpdatedAt,
                product.CategoryId,
                category?.Name ?? "N/A",
                product.ProductVariants.Select(v => new ProductVariantIntegrationModel(
                    v.Id, v.Sku, v.Name, v.Attributes, v.BasePrice, v.Cost, v.IsActive ?? true
                )).ToList()
            );

            // Publish qua Kafka (Thực tế là lưu vào bảng cap.published trước)
            await _eventBus.PublishAsync("product.persisted", integrationEvent, cancellationToken);

            // 6. Chốt sổ (Commit DB + Outbox)
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success(_mapper.Map<CreateProductCommandResponse>(product));
        }
        catch (Exception)
        {
            await _uow.RollbackAsync(cancellationToken);
            throw;
        }

        //var product = _mapper.Map<Product>(request);

        //// Thêm ảnh vào cho sản phẩm

        //if (request.ImageFile != null)
        //{
        //    var uploadResult = await _cloudinaryService.UploadImageAsync(request.ImageFile);
        //    if (uploadResult != null)
        //    {
        //        product.ImageUrl = uploadResult.Url;
        //        product.ImagePublicId = uploadResult.PublicId;
        //    }
        //}

        //await _productRepository.AddAsync(product);

        //return Result.Success(_mapper.Map<CreateProductCommandResponse>(product));
    }
}