using System;

namespace ProductService.Application.DTOs.Requests;

public class GetProductsRequest
{
    public Guid? CategoryId { get; set; }
}
