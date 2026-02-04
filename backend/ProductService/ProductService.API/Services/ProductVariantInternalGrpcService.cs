using AutoMapper;
using Grpc.Core;
using MediatR;
using ProductService.Application.Features.ProductVariants.Queries.GetById;
using ProductService.Grpc;

namespace ProductService.API.Services
{
    public class ProductVariantInternalGrpcService : ProductVariantGrpc.ProductVariantGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ProductVariantInternalGrpcService(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public override async Task<GetProductVariantByIdResponse> GetById(
        GetProductVariantByIdRequest request,
        ServerCallContext context)
        {
            if (!Guid.TryParse(request.ProductVariantId, out var id))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Invalid ProductVariantId"));
            }

            var result = await _mediator.Send(
                new GetProductVariantByIdQuery(id),
                context.CancellationToken);

            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, result.Errors.First()));
            }

            var data = result.Value;

            return new GetProductVariantByIdResponse
            {
                Id = data.Id.ToString(),
                Sku = data.Sku,
                Name = data.Name ?? string.Empty,
                Attributes = data.Attributes ?? string.Empty,
                BasePrice = (double)data.BasePrice,
                Cost = (double)data.Cost,
                IsActive = data.IsActive ?? false,
                UpdatedAt = data.UpdatedAt.ToString("O"),
                ProductId = data.ProductId.ToString()
            };
        }
    }
}
