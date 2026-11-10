using Ardalis.Result;
using AutoMapper;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetByProductName;

public sealed class GetProductByNameQueryHandler(
    IProductRepository productRepository, IMapper mapper
) : IRequestHandler<GetProductByNameQuery, Result<List<GetProductByNameQueryResponse>>>
{
    public async Task<Result<List<GetProductByNameQueryResponse>>> Handle(
    GetProductByNameQuery request,
    CancellationToken cancellationToken)
    {
        if (request.ProductName == null)
        {
            return Result.Invalid(
                    new ValidationError("Product name cannot be empty."));
        }

        var products = await productRepository
            .GetProductByNameContainingAsync(request.ProductName);

        if (products == null || products.Count == 0)
        {
            return Result.NotFound(
                $"No product found with name '{request.ProductName}'");
        }

        var items = mapper.Map<List<GetProductByNameQueryResponse>>(products);

        return Result.Success(items);
    }
}