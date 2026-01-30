using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Products.Command.DeleteProduct;

public sealed record DeleteProductCommand(Guid ProductId)
    : IRequest<Result<bool>>;
