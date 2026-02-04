using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Commands.CreateRecipe
{
    public class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, Unit>
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IIngredientRepository _ingredientRepository;
        public CreateRecipeHandler(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository)
        {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
        }
        public async Task<Unit> Handle(CreateRecipeCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            //ktra variantID tồn tại chưa
            if((await _recipeRepository.GetByIdAsync(request.VariantId)) != null)
            {
                throw new DirectoryNotFoundException(Message.Recipe.AlreadyExists);
            }
            List<Domain.Entities.Recipe> existingRecipes = [];
            foreach (var ingredient in request.Ingredients)
            {
                var ingredientExist = await _ingredientRepository.GetByIdAsync(ingredient.IngredientId);
                if (ingredientExist == null)
                    throw new DirectoryNotFoundException(Message.Ingredient.NotFound);

                existingRecipes.Add(new Domain.Entities.Recipe
                {
                    VariantId = request.VariantId,
                    IngredientId = ingredient.IngredientId,
                    Quantity = ingredient.Quantity
                });
            }

            await _recipeRepository.AddRangeAsync(existingRecipes);
            return Unit.Value;
        }
    }
}
