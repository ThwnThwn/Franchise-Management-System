namespace ProductService.Application.Features.Products.Queries.GetByKeyword;

using AutoMapper;
using BuildingBlocks.Core.Pagination;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.SearchEngines;

public sealed class GetProductByKeywordQueryHandler :
    IRequestHandler<GetProductByKeywordQuery, PagedResult<ProductDto>>
{
    private readonly IProductSearchEngine _productSearchEngine;
    private readonly IMapper _mapper;

    public GetProductByKeywordQueryHandler(IProductSearchEngine productSearchEngine, IMapper mapper)
    {
        _productSearchEngine = productSearchEngine;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(
        GetProductByKeywordQuery query,
        CancellationToken cancellationToken)
    {
        var pageResult = await _productSearchEngine.GetAllProductWithPagingByAsync(
            query.Keyword,
            query.CategoryId,
            query.Page,
            query.PageSize,
            cancellationToken);

        return pageResult;
        //var items = pageResult.Items
        //    .Select(p => _mapper.Map<ProductDto>(p))
        //    .ToList();

        //return new PagedResult<ProductDto>(
        //    items,
        //    pageResult.TotalCount,
        //    pageResult.Page,
        //    pageResult.PageSize);
    }
}