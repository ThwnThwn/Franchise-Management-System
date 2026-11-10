using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.Categories.Queries.GetById.GetCategoryByIdQuery;

namespace ProductService.Application.Features.Categories.Queries.GetById;

public sealed record GetCategoryByIdQuery(Guid CategoryId) :
    IRequest<Result<GetCategoryByIdResponse>>
{
    public sealed class GetCategoryByIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}