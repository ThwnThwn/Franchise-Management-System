using InventoryService.Application.Dtos.Ingredient;
using MediatR;

namespace InventoryService.Application.Features.Ingrdient.Queries.GetIngredientById;

public class GetIngredientByIdQuery : IRequest<IngredientViewRes?>
{
    public Guid Id { get; set; }

    public GetIngredientByIdQuery(Guid id)
    {
        Id = id;
    }
}