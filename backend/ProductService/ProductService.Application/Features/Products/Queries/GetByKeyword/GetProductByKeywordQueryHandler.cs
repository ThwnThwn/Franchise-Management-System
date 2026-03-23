namespace ProductService.Application.Features.Products.Queries.GetByKeyword;

using BuildingBlocks.Core.Pagination;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Repositories;

public sealed class GetProductByKeywordQueryHandler(
    IProductRepository repo
) : IRequestHandler<GetProductByKeywordQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        GetProductByKeywordQuery query,
        CancellationToken cancellationToken)
    {
        var pageResult = await repo.GetAllProductWithPagingByAsync(
            query.Keyword,
            query.CategoryId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = pageResult.Items
            .Select(product =>
            {
                return new ProductDto
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
            })
            .ToList();

        return new PagedResult<ProductDto>(
            items,
            pageResult.TotalCount,
            pageResult.Page,
            pageResult.PageSize);
    }
}