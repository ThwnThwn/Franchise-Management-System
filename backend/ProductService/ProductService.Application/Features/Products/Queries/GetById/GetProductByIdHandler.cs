using Ardalis.Result;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetById;

public sealed class GetProductByIdHandler(
    IProductRepository repo
) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await repo.GetProductByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.NotFound($"Product with id {request.ProductId} not found.");
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            IsDeleted = product.IsDeleted,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,

            Variants = product.ProductVariants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name,
                Attributes = v.Attributes,
                BasePrice = v.BasePrice,
                Cost = v.Cost,
                IsActive = v.IsActive,
                Sku = v.Sku
            }).ToList()
        };

        return Result.Success(dto);
    }
}