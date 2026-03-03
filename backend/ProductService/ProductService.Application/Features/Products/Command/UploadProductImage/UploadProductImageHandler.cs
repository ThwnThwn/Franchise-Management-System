using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Application.Repositories;
using SharedKernel.Events;

namespace ProductService.Application.Features.Products.Command.UploadProductImage;

public class UploadProductImageHandler(
    IProductRepository productRepository,
    ICloudinaryService cloudinaryService,
    IProductUow uow,
    IIntegrationEventBus eventBus
) : IRequestHandler<UploadProductImageCommand, Result<UploadProductImageResponse>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    private readonly IProductUow _uow = uow;
    private readonly IIntegrationEventBus _eventBus = eventBus;

    public async Task<Result<UploadProductImageResponse>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        // Lấy Product (Kèm cả Variant nếu repository của bro chưa Include mặc định)
        var product = await _productRepository.GetProductByIdAsync(request.ProductId);
        if (product is null)
        {
            return Result.NotFound("Product not found");
        }
        var oldPublicId = product.ImagePublicId;

        // Upload ảnh mới
        var uploadResult = await _cloudinaryService.UploadImageFromBytesAsync(
            request.ImageData,
            request.FileName,
            request.ContentType);

        // Cập nhật state trên memory
        product.ImageUrl = uploadResult.Url;
        product.ImagePublicId = uploadResult.PublicId;

        // BẮT ĐẦU TRANSACTION ĐỂ LƯU DB VÀ BẮN KAFKA
        await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            // Cập nhật vào DB
            await _productRepository.UpdateAsync(product);

            // Chuẩn bị Integration Event
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
                product.Category.Name,
                product.ProductVariants?.Select(v => new ProductVariantIntegrationModel(
                    v.Id, v.Sku, v.Name, v.Attributes, v.BasePrice, v.Cost, v.IsActive ?? true
                )).ToList() ?? new List<ProductVariantIntegrationModel>()
            );

            await _eventBus.PublishAsync("product.persisted", integrationEvent, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Xóa ảnh cũ trên Cloudinary
            if (!string.IsNullOrWhiteSpace(oldPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(oldPublicId);
            }

            return Result.Success(new UploadProductImageResponse(
                ProductId: product.Id,
                ImageUrl: product.ImageUrl,
                ImagePublicId: product.ImagePublicId
            ));
        }
        catch (Exception)
        {
            await _uow.RollbackAsync(cancellationToken);
            // await _cloudinaryService.DeleteImageAsync(uploadResult.PublicId);
            throw;
        }
    }
}