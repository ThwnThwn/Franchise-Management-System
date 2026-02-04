using AutoMapper;
using InventoryService.Application.Dtos.Recipe;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Queries.GetByVariantIds
{
    public class GetByVariantIdsHandler : IRequestHandler<GetByVariantIdsQuery, List<RecipeViewRes>>
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IMapper _mapper;
        public GetByVariantIdsHandler(IRecipeRepository recipeRepository, IMapper mapper)
        {
            _recipeRepository = recipeRepository;
            _mapper = mapper;
        }
        public async Task<List<RecipeViewRes>> Handle(GetByVariantIdsQuery request, CancellationToken cancellationToken)
        {
            var recipes = await _recipeRepository.GetRecipesByVariantIdsAsync(request.VariantIds, cancellationToken);
            return (recipes != null || !recipes!.Any()) ? recipes!.Select(i => _mapper.Map<RecipeViewRes>(i)).ToList() : new List<RecipeViewRes>();
        }
    }
}
