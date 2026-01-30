using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetByProductName;

public sealed class GetProductByNameQueryHandler
    : IRequestHandler<GetProductByNameQuery, Result<List<GetProductByNameQueryResponse>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByNameQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<GetProductByNameQueryResponse>>> Handle(
    GetProductByNameQuery request,
    CancellationToken cancellationToken)
    {
        if(request.productName == null)
        {
            return Result.Invalid(
                    new ValidationError("Product name cannot be empty."));
        }

        var products = await _productRepository
            .GetProductByNameContainingAsync(request.productName);

        if (products == null || !products.Any())
        {
            return Result.NotFound(
                $"No product found with name '{request.productName}'");
        }

        var items = _mapper.Map<List<GetProductByNameQueryResponse>>(products);

        return Result.Success(items);

    }

}
