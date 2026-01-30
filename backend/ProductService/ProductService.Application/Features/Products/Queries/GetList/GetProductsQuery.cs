using System;
using System.Collections.Generic;
using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Products.Queries.GetList;

// Query lấy danh sách sản phẩm (menu)
public sealed record GetProductsQuery(Guid? CategoryId)
    : IRequest<Result<GetProductsResultDto>>;

public sealed class GetProductsResultDto
{
    public List<GetProductListItemDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
}

public sealed class GetProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;

    public List<ProductSizeListItemDto> Sizes { get; set; } = new();
}

public sealed class ProductSizeListItemDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = default!;
    public string? Name { get; set; }
    public string? Attributes { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}

