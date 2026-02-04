using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Products.Queries.GetByProductName
{
    public sealed record GetProductByNameQuery(string productName)
    : IRequest<Result<List<GetProductByNameQueryResponse>>>;
    public sealed record GetProductsByNameResult
    {
        public List<GetProductByNameQueryResponse> Products { get; init; } = new();
        public int TotalCount { get; init; }
    }

    public sealed class GetProductByNameQueryResponse
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
        public string ImagePublicId { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public Guid CategoryId { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? UpdatedAt { get; init; }
        public List<ProductVariantDto> Variants { get; init; } = new();
    }

    public sealed class ProductVariantDto
    {
        public Guid Id { get; init; }
        public string? Sku { get; init; }
        public string? Name { get; init; }
        public string? Attributes { get; init; }
        public decimal? BasePrice { get; init; }
        public decimal? Cost { get; init; }
        public bool? IsActive { get; init; }
    }

}

