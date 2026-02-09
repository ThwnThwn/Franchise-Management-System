using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.ProductVariants.Queries.GetById.GetProductVariantByIdQuery;

namespace ProductService.Application.Features.ProductVariants.Queries.GetById;

public sealed record GetProductVariantByIdQuery(Guid ProductVariantId):
    IRequest<Result<GetProductVariantByIdQueryResponse>>
{
    public sealed class GetProductVariantByIdQueryResponse
    {
        public Guid Id { get; set; }

        public string Sku { get; set; } = null!;

        public string? Name { get; set; }

        public string? Attributes { get; set; }

        public decimal BasePrice { get; set; }

        public decimal Cost { get; set; }

        public bool? IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Guid ProductId { get; set; }
    }
}
