using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Products.Queries.GetById;

// Query
public sealed record GetProductByIdQuery(Guid ProductId)
    : IRequest<Result<GetProductDetailDto>>;

// DTO chi tiết sản phẩm
public sealed class GetProductDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;

    public List<ProductSizeDto> Sizes { get; set; } = new();
}

public sealed class ProductSizeDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = default!;
    public string? Name { get; set; }
    public string? Attributes { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Cost { get; set; }
    public bool IsActive { get; set; }
}

