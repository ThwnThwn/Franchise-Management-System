using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.SearchEngines;

namespace ProductService.Application.Features.Products.Queries.GetById;

public sealed class GetProductByIdHandler
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductSearchEngine _productSearchEngine;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IProductSearchEngine productSearchEngine, IMapper mapper)
    {
        _productSearchEngine = productSearchEngine;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productSearchEngine.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.NotFound($"Product with id {request.ProductId} not found.");
        }

        return Result.Success(product);
    }
}