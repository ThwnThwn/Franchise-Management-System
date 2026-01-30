using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetList;

public sealed class GetProductsHandler
    : IRequestHandler<GetProductsQuery, Result<GetProductsResultDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetProductsResultDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllProductsAsync(request.CategoryId);
        var productDtos = _mapper.Map<List<GetProductListItemDto>>(products);

        var result = new GetProductsResultDto
        {
            Products = productDtos,
            TotalCount = productDtos.Count
        };

        return Result.Success(result);
    }
}

