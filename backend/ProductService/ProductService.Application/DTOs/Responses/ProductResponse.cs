using System;
using System.Collections.Generic;

namespace ProductService.Application.DTOs.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public List<ProductSizeResponse> Sizes { get; set; } = new();
}

public class ProductSizeResponse
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Name { get; set; }
    public string? Attributes { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}
