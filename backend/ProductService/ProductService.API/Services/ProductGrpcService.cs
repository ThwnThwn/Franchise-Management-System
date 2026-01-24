using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.Application.Features.Products.GetById;
using ProductService.Application.Features.Products.GetList;

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

    public override async Task<GetProductsReply> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            if (!Guid.TryParse(request.CategoryId, out var parsed))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "CategoryId passed is not a valid GUID"));
            }
            categoryId = parsed;
        }

        var query = new GetProductsQuery(categoryId);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "Failed to get products";
            throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        }

        var reply = new GetProductsReply
        {
            TotalCount = result.Value.TotalCount
        };

        reply.Products.AddRange(_mapper.Map<List<Product>>(result.Value.Products));

        return reply;
    }

    public override async Task<GetProductByIdReply> GetProductById(GetProductByIdRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || !Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "id must be a valid GUID"));
        }

        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                throw new RpcException(new Status(StatusCode.NotFound, result.Errors.FirstOrDefault() ?? "Product not found"));
            }
            throw new RpcException(new Status(StatusCode.Internal, result.Errors.FirstOrDefault() ?? "Failed to get product"));
        }

        var reply = new GetProductByIdReply
        {
            Product = _mapper.Map<Product>(result.Value)
        };

        return reply;
    }
}

