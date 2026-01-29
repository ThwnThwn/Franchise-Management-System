using Ardalis.Result;
using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.Application.Features.Products.Command.CreateProduct;
using ProductService.Application.Features.Products.Command.DeleteProduct;
using ProductService.Application.Features.Products.Command.UpdateProduct;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Domain.Entities;

namespace ProductService.API.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ProductGrpcService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    // -------------------------
    // GET LIST (by category)
    // -------------------------
    public override async Task<GetProductsReply> GetProducts(
        GetProductsRequest request,
        ServerCallContext context)
    {
        Guid? categoryId = null;

        if (!string.IsNullOrWhiteSpace(request.CategoryId) &&
            Guid.TryParse(request.CategoryId, out var parsed))
        {
            categoryId = parsed;
        }

        var query = new GetProductsQuery(categoryId);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.Internal,
                    result.Errors.FirstOrDefault() ?? "Failed to get products"));
        }

        var reply = new GetProductsReply
        {
            TotalCount = result.Value.TotalCount
        };

        reply.Products.AddRange(
            _mapper.Map<List<Product>>(result.Value.Products));

        return reply;
    }

    // -------------------------
    // GET BY ID
    // -------------------------
    public override async Task<GetProductByIdReply> GetProductById(
        GetProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var productId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid product id"));
        }

        var result = await _mediator.Send(
            new GetProductByIdQuery(productId),
            context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.NotFound)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found"));
            }

            throw new RpcException(
                new Status(StatusCode.Internal,
                    result.Errors.FirstOrDefault() ?? "Get product failed"));
        }

        return new GetProductByIdReply
        {
            Product = _mapper.Map<Product>(result.Value)
        };
    }

    public override async Task<GetProductsReply> GetProductsByName(
        GetProductsByNameRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Name is required"));
        }

        var result = await _mediator.Send(
            new GetProductByNameQuery(request.Name),
            context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.Internal,
                    result.Errors.FirstOrDefault() ?? "Search failed"));
        }

        var reply = new GetProductsReply
        {
            TotalCount = result.Value.TotalCount
        };

        reply.Products.AddRange(
            _mapper.Map<List<Product>>(result.Value.Products));

        return reply;
    }

    public override async Task<CreateProductReply> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.CategoryId, out var categoryId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid categoryId"));
        }

        var command = new CreateProductCommand(
            new CreateProductCommand.CreateProductCommandRequest(
                request.Name,
                request.Description,
                request.ImageUrl,
                request.ImagePublicId,
                categoryId,
                request.Variants.Any()
                    ? request.Variants.Select(v =>
                        new CreateProductCommand.CreateProductVariantRequest
                        {
                            Sku = v.Sku,
                            Name = v.Name,
                            Attributes = v.Attributes,
                            BasePrice = (decimal?)v.BasePrice,
                            Cost = (decimal?)v.Cost,
                            IsActive = v.IsActive
                        }).ToList()
                    : null
            )
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            throw new RpcException(
                new Status(StatusCode.Internal,
                    result.Errors.FirstOrDefault() ?? "Create product failed"));
        }

        return new CreateProductReply
        {
            ProductId = result.Value.ProductId.ToString(),
            Name = result.Value.Name,
            Description = result.Value.Description,
            ImageUrl = result.Value.ImageUrl,
            ImagePublicId = result.Value.ImagePublicId,
            IsActive = result.Value.IsActive ?? true,
            CategoryId = result.Value.CategoryId.ToString(),
            CreatedAt = result.Value.CreatedAt.ToString("O"),
            VariantIds =
            {
                result.Value.VariantIds?
                    .Select(x => x.ToString()) ?? Enumerable.Empty<string>()
            }
        };
    }

    // -------------------------
    // UPDATE
    // -------------------------
    public override async Task<UpdateProductReply> UpdateProduct(
        UpdateProductRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid productId"));
        }

        Guid categoryId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(request.CategoryId) &&
            !Guid.TryParse(request.CategoryId, out categoryId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid categoryId"));
        }

        var command = new UpdateProductCommand(
            productId,
            new UpdateProductCommand.UpdateProductCommandRequest(
                productId,
                request.Name,
                request.Description,
                request.ImageUrl,
                request.ImagePublicId,
                request.IsActive,
                categoryId,
                request.Variants.Any()
                    ? request.Variants.Select(v =>
                        new UpdateProductCommand.UpdateProductVariantRequest(
                            v.Sku,
                            v.Name,
                            v.Attributes,
                            (decimal?)v.BasePrice,
                            (decimal?)v.Cost,
                            v.IsActive
                        )).ToList()
                    : null
            )
        );

        var result = await _mediator.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.NotFound)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found"));
            }

            throw new RpcException(
                new Status(StatusCode.Internal,
                    result.Errors.FirstOrDefault() ?? "Update product failed"));
        }

        return new UpdateProductReply
        {
            ProductId = result.Value.ProductId.ToString(),
            Name = result.Value.Name,
            Description = result.Value.Description,
            ImageUrl = result.Value.ImageUrl,
            ImagePublicId = result.Value.ImagePublicId,
            IsActive = result.Value.IsActive,
            CategoryId = result.Value.CategoryId.ToString(),
            UpdatedAt = result.Value.UpdatedAt.ToString("O")
        };
    }

    // -------------------------
    // DELETE
    // -------------------------
    public override async Task<DeleteProductReply> DeleteProduct(
        DeleteProductRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var productId))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid product id"));
        }

        var result = await _mediator.Send(
            new DeleteProductCommand(productId),
            context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == ResultStatus.NotFound)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, "Product not found"));
            }

            throw new RpcException(
                new Status(StatusCode.Internal, "Delete product failed"));
        }

        return new DeleteProductReply { Success = true };
    }
}
