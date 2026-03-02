using Ardalis.Result;
using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.Features.Products.Queries.GetById;

// Query
public sealed record GetProductByIdQuery(Guid ProductId)
    : IRequest<Result<ProductDto>>;