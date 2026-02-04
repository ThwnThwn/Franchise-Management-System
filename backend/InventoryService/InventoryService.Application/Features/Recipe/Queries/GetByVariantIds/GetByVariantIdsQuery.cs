using InventoryService.Application.Dtos.Recipe;
using MediatR;

namespace InventoryService.Application.Features.Recipe.Queries.GetByVariantIds
{
    public record class GetByVariantIdsQuery(IEnumerable<Guid> VariantIds) : IRequest<List<RecipeViewRes>>
    {
        public IEnumerable<Guid> VariantIds { get; init; } = VariantIds ?? Enumerable.Empty<Guid>();
    }
}
