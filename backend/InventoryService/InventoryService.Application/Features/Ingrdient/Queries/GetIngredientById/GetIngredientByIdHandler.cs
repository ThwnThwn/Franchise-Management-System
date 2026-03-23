using AutoMapper;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;

namespace InventoryService.Application.Features.Ingrdient.Queries.GetIngredientById;

public class GetIngredientByIdHandler : IRequestHandler<GetIngredientByIdQuery, IngredientViewRes?>
{
    private readonly IIngredientRepository _ingredientRepository;
    private readonly IMapper _mapper;

    public GetIngredientByIdHandler(IIngredientRepository ingredientRepository, IMapper mapper)
    {
        _ingredientRepository = ingredientRepository;
        _mapper = mapper;
    }

    public async Task<IngredientViewRes?> Handle(GetIngredientByIdQuery request, CancellationToken cancellationToken)
    {
        var ingredient = await _ingredientRepository.GetByIdAsync(request.Id);
        if (ingredient is null) return null;

        return _mapper.Map<IngredientViewRes>(ingredient);
    }
}