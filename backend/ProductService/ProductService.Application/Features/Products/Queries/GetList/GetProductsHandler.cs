using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetList;

public sealed class GetProductsHandler(
    IProductRepository productRepository, IMapper mapper
) : IRequestHandler<GetProductsQuery, Result<GetProductsResultDto>>
{
    public async Task<Result<GetProductsResultDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllProductsAsync(request.CategoryId);
        var productDtos = mapper.Map<List<GetProductListItemDto>>(products);

        var result = new GetProductsResultDto
        {
            Products = productDtos,
            TotalCount = productDtos.Count
        };

        return Result.Success(result);
    }
}