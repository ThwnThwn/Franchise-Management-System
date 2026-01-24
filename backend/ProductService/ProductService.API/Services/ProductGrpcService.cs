using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using ProductService.Application.Services;
using AppRequests = ProductService.Application.DTOs.Requests;

namespace ProductService.API.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly ProductApplicationService _productService;
    private readonly IMapper _mapper;

    public ProductGrpcService(ProductApplicationService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public override async Task<GetProductsReply> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            if (!Guid.TryParse(request.CategoryId, out var parsed))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "category_id must be a valid GUID"));
            }
            categoryId = parsed;
        }

        var result = await _productService.GetProductsAsync(new AppRequests.GetProductsRequest { CategoryId = categoryId });

        var reply = new GetProductsReply
        {
            TotalCount = result.TotalCount
        };

        reply.Products.AddRange(_mapper.Map<List<Product>>(result.Products));

        return reply;
    }

    public override async Task<GetProductByIdReply> GetProductById(GetProductByIdRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) || !Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "id must be a valid GUID"));
        }

        var product = await _productService.GetProductByIdAsync(new AppRequests.GetProductByIdRequest { Id = id });
        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
        }

        var reply = new GetProductByIdReply
        {
            Product = _mapper.Map<Product>(product)
        };

        return reply;
    }
}

