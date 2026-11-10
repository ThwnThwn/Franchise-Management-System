using BuildingBlocks.Core.Pagination;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Mapping;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetByKeyword;

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
            .Select(product => product.MapToProductDto())
            .ToList();

        return new PagedResult<ProductDto>(
            items,
            pageResult.TotalCount,
            pageResult.Page,
            pageResult.PageSize);
    }
}