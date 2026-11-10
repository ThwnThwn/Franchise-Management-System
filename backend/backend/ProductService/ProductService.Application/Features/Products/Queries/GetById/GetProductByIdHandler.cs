using Ardalis.Result;
using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Mapping;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.Products.Queries.GetById;

public sealed class GetProductByIdHandler(
    IProductRepository repo
) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await repo.GetProductByIdAsync(request.ProductId);

        if (product is null)
        {
            return Result.NotFound($"Product with id {request.ProductId} not found.");
        }

        var dto = product.MapToProductDto();

        return Result.Success(dto);
    }
}