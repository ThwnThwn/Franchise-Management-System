namespace ProductService.Application.Dtos
{
    public sealed record ProductDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public List<ProductVariantDto> Variants { get; init; } = [];
    }

    public sealed class ProductVariantDto
    {
        public Guid Id { get; init; }
        public string Sku { get; init; } = null!;
        public string? Name { get; init; }
        public string? Attributes { get; init; }
        public decimal BasePrice { get; init; }
        public decimal Cost { get; init; }
        public bool? IsActive { get; init; }
    }
}