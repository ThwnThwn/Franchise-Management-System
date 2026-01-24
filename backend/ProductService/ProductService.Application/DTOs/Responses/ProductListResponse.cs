using System.Collections.Generic;

namespace ProductService.Application.DTOs.Responses;

public class ProductListResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int TotalCount { get; set; }
}
