using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Command.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IMapper mapper
) : IRequestHandler<UpdateProductCommand, Result<UpdateProductCommandResponse>>
{
    public async Task<Result<UpdateProductCommandResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var tmpProd = await productRepository
            .GetProductByNameAsync(request.Request.Name ?? "", request.ProductId);
        if (tmpProd is not null)
        {
            return Result.Conflict($"Tên sản phẩm '{request.Request.Name}' đã tồn tại trong hệ thống.");
        }

        var checkCategory = await categoryRepository.GetByIdAsync(request.Request.CategoryId);
        if (checkCategory is null)
        {
            return Result.NotFound($"Không tìm thấy danh mục với mã: '{request.Request.CategoryId}'.");
        }

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
                }).ToList() ?? []
            };

            await productRepository.UpdateWithVariantsAsync(product);

            return Result.Success(mapper.Map<UpdateProductCommandResponse>(product));
        }
        catch (Exception ex)
        {
            // Trả về lỗi tiếng Việt nếu DB có vấn đề
            return Result.Error($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
        }
    }
}