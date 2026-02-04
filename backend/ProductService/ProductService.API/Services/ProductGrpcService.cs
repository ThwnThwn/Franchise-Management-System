using Ardalis.Result;
using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.Application.Features.Products.Command.CreateProduct;
using ProductService.Application.Features.Products.Command.DeleteProduct;
using ProductService.Application.Features.Products.Queries.GetById;
using ProductService.Application.Features.Products.Queries.GetByKeyword;
using ProductService.Application.Features.Products.Queries.GetByProductName;
using ProductService.Application.Features.Products.Queries.GetList;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.API.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductGrpcService(IMediator mediator, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

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
            TotalCount = result.Value.Count
        };

        reply.Products.AddRange(
            _mapper.Map<List<Product>>(result.Value));

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
            new CreateProductCommand.CreateProductCommandRequest
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ImagePublicId = request.ImagePublicId,
                ImageFile = null, // gRPC không hỗ trợ upload file trực tiếp
                CategoryId = categoryId,
                Variants = request.Variants.Any()
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
            }
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
    public override async Task<GetProductsByKeywordReply>
GetProductsByKeyword(
    GetProductsByKeywordRequest request,
    ServerCallContext context)
    {
        Guid? categoryId = null;

        if (Guid.TryParse(request.CategoryId, out var parsed))
            categoryId = parsed;

        var query = new GetProductByKeywordQuery(
            request.Keyword,
            categoryId,
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10);

        var result = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetProductsByKeywordReply
        {
            TotalCount = (int)result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage
        };

        reply.Products.AddRange(
            _mapper.Map<List<Product>>(result.Items));

        return reply;
    }


    public override async Task<UploadImageReply> UploadImage(
        UploadImageRequest request,
        ServerCallContext context)
    {
        // Validate request
        if (request.ImageData == null || request.ImageData.Length == 0)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Image data is required"));
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "File name is required"));
        }

        try
        {
            // Upload image to Cloudinary
            var uploadResult = await _cloudinaryService.UploadImageFromBytesAsync(
                request.ImageData.ToByteArray(),
                request.FileName,
                request.ContentType ?? "image/png");

            return new UploadImageReply
            {
                Url = uploadResult.Url,
                PublicId = uploadResult.PublicId
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(
                new Status(StatusCode.Internal, $"Upload image failed: {ex.Message}"));
        }
    }
}
