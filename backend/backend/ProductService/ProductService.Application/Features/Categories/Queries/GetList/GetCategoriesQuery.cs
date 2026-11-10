using Ardalis.Result;
using MediatR;

namespace ProductService.Application.Features.Categories.Queries.GetList;

public class GetCategoriesQuery : IRequest<Result<GetCategoriesResultDto>>
{
}

public sealed class GetCategoriesResultDto
{
    public List<GetCategoryListItemDto> Categories { get; set; } = [];
}

public sealed class GetCategoryListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}