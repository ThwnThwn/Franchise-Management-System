using Ardalis.Result;
using MediatR;
using ProductService.Application.Repositories;

namespace ProductService.Application.Features.ProductVariants.Queries.GetById
{
    public sealed class GetProductVariantByIdQueryHandler :
        IRequestHandler<
            GetProductVariantByIdQuery,
            Result<GetProductVariantByIdQuery.GetProductVariantByIdQueryResponse>>
    {
        private readonly IProductVariantRepository _repository;

        public GetProductVariantByIdQueryHandler(IProductVariantRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<GetProductVariantByIdQuery.GetProductVariantByIdQueryResponse>> Handle(
            GetProductVariantByIdQuery query,
            CancellationToken cancellationToken)
        {
            var productVariant =
                await _repository.GetProductVariantByIdAsync(query.ProductVariantId);

            if (productVariant == null)
            {
                return Result.NotFound(
                    $"Product Variant with id '{query.ProductVariantId}' not found");
            }

            var response =
                new GetProductVariantByIdQuery.GetProductVariantByIdQueryResponse
                {
                    Id = productVariant.Id,
                    Sku = productVariant.Sku,
                    Name = productVariant.Name,
                    Attributes = productVariant.Attributes,
                    BasePrice = productVariant.BasePrice,
                    Cost = productVariant.Cost,
                    IsActive = productVariant.IsActive,
                    CreatedAt = productVariant.CreatedAt,
                    UpdatedAt = productVariant.UpdatedAt,
                    ProductId = productVariant.ProductId
                };

            return Result.Success(response);
        }
    }
}
