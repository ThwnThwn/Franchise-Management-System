using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.Products.Command.CreateProduct.CreateProductCommand;

namespace ProductService.Application.Features.Products.Command.CreateProduct
{
    public sealed record CreateProductCommand(CreateProductCommandRequest Request)
        : IRequest<Result<CreateProductCommandResponse>>
    {
        public sealed record CreateProductCommandRequest(
            string Name,
            string? Description,
            string? ImageUrl,
            string? ImagePublicId,
            Guid CategoryId,
            List<CreateProductVariantRequest>? Variants
        );


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
