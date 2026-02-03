using BuildingBlocks.Core.Pagination;
using MediatR;

namespace ProductService.Application.Features.Products.Queries.GetByKeyword
{
    public sealed record GetProductByKeywordQuery(
    string? Keyword,
    Guid? CategoryId,
    int Page,
    int PageSize
) : IRequest<PagedResult<GetProductByKeywordQueryResponse>>;

    public sealed record GetProductByKeywordQueryResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImagePublicId { get; set; }

        public bool? IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Guid CategoryId { get; set; }

        public List<ProductVariantDTO> Variants { get; init; } = new();
    }

    public sealed class ProductVariantDTO
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
