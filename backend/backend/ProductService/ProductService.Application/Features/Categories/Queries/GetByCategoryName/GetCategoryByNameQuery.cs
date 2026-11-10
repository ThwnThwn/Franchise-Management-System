using Ardalis.Result;
using MediatR;
using static ProductService.Application.Features.Categories.Queries.GetByCategoryName.GetCategoryByNameQuery;

namespace ProductService.Application.Features.Categories.Queries.GetByCategoryName;

public sealed record GetCategoryByNameQuery(string CategoryName) :
   IRequest<Result<GetCategoryByNameQueryResponse>>
{
    public sealed class GetCategoryByNameQueryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}