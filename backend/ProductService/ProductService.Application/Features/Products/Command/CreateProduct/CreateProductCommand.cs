using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;

using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Features.Products.Command.CreateProduct
{
    public sealed record CreateProductCommand(CreateProductCommandRequest Request)
        : IRequest<Result<CreateProductCommandResponse>>
    {
        public sealed class CreateProductCommandRequest
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public string? ImagePublicId { get; set; }
            public IFormFile? ImageFile { get; set; }
            public Guid CategoryId { get; set; }
            public List<CreateProductVariantRequest>? Variants { get; set; }
        }


        public sealed class CreateProductVariantRequest
        {
            public string? Sku { get; set; } = default!; // add '{}' to test success
            public string? Name { get; set; }
            public string? Attributes { get; set; } // add '{}' to test success
            public decimal? BasePrice { get; set; }
            public decimal? Cost { get; set; }
            public bool? IsActive { get; set; } = true;
        }

        public sealed class CreateProductCommandResponse
        {
            public Guid ProductId { get; set; }
            public string Name { get; set; } = default!;
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public string? ImagePublicId { get; set; }
            public bool? IsActive { get; set; }
            public Guid CategoryId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public List<Guid>? VariantIds { get; set; } = new();
        }
    }
}
