using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Features.ProductVariants.Queries.GetById;
using ProductService.Grpc;
using ProductService.Infrastructure.ApplicationDbContext;

namespace ProductService.API.Services
{
    public class ProductVariantInternalGrpcService : ProductVariantGrpc.ProductVariantGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IProductDbContext _dbContext;

        public ProductVariantInternalGrpcService(IMediator mediator, IMapper mapper, IProductDbContext dbContext)
        {
            _mediator = mediator;
            _mapper = mapper;
            _dbContext = dbContext;
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

        /// <summary>
        /// Nhận batch cập nhật giá vốn từ InventoryService (sau khi tính MAC).
        /// Cập nhật trường Cost của ProductVariant trong DB.
        /// </summary>
        public override async Task<Empty> UpdateVariantCosts(
            UpdateVariantCostsRequest request,
            ServerCallContext context)
        {
            if (request.Updates == null || !request.Updates.Any())
                return new Empty();

            var variantIds = request.Updates
                .Select(u => Guid.Parse(u.VariantId))
                .ToList();

            var variants = await _dbContext.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToListAsync(context.CancellationToken);

            foreach (var update in request.Updates)
            {
                var variantId = Guid.Parse(update.VariantId);
                var variant = variants.FirstOrDefault(v => v.Id == variantId);
                if (variant != null)
                {
                    variant.Cost = (decimal)update.NewCost;
                }
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            return new Empty();
        }
    }
}

