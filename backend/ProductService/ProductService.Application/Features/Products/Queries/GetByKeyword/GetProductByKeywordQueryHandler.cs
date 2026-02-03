namespace ProductService.Application.Features.Products.Queries.GetByKeyword;

using AutoMapper;
using BuildingBlocks.Core.Pagination;
using MediatR;
using ProductService.Application.Repositories;

    public sealed class GetProductByKeywordQueryHandler:
        IRequestHandler<GetProductByKeywordQuery, PagedResult<GetProductByKeywordQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
    public GetProductByKeywordQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

    public async Task<PagedResult<GetProductByKeywordQueryResponse>> Handle(
        GetProductByKeywordQuery query,
        CancellationToken cancellationToken)
    {
        var pageResult = await _productRepository.GetAllProductWithPagingByAsync(
            query.Keyword,
            query.CategoryId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = pageResult.Items
            .Select(p => _mapper.Map<GetProductByKeywordQueryResponse>(p))
            .ToList();

        return new PagedResult<GetProductByKeywordQueryResponse>(
            items,
            pageResult.TotalCount,
            pageResult.Page,
            pageResult.PageSize);
    }
}

