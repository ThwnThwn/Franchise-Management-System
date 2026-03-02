using BuildingBlocks.Core.Pagination;
using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetByKeyword
{
    public sealed record GetProductByKeywordQuery(
        string? Keyword,
        Guid? CategoryId,
        int Page,
        int PageSize
    ) : IRequest<PagedResult<ProductDto>>;
}