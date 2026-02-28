using Ardalis.Result;
using MediatR;
using static UpdateProductCommand;

public sealed record UpdateProductCommand(
    Guid ProductId,
    UpdateProductCommandRequest Request
) : IRequest<Result<UpdateProductCommandResponse>>
{
    public sealed record UpdateProductCommandRequest(
        Guid ProductId,
        string? Name,
        string? Description,
        //string? ImageUrl,
        //string? ImagePublicId,
        bool? IsActive,
        Guid CategoryId,
        List<UpdateProductVariantRequest>? Variants
    );

    public sealed record UpdateProductVariantRequest(
        string? Sku,
        string? Name,
        string? Attributes,
        decimal? BasePrice,
        decimal? Cost,
        bool? IsActive
    );

    public sealed record UpdateProductCommandResponse
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImagePublicId { get; init; }
        public bool IsActive { get; init; }
        public Guid CategoryId { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}