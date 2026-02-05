using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Recipe.Commands.MutationRecipe
{
    public class MutationRecipeHandler : IRequestHandler<MutationRecipeCommand, Unit>
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IProductServiceClient _productServiceClient;
        public MutationRecipeHandler(IRecipeRepository recipeRepository, IProductServiceClient productServiceClient)
        {
            _recipeRepository = recipeRepository;
            _productServiceClient = productServiceClient;
        }
        public async Task<Unit> Handle(MutationRecipeCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            //ktra variant có tồn tại hay k
            var variant = await _productServiceClient.GetVariantById(request.VariantId);
            if (variant == null)
            {
                throw new DirectoryNotFoundException(Message.ProductVariant.NotFound);
            }
            //lấy recipe ra từ db
            var recipe = await _recipeRepository.GetRecipesByVariantIdAsync(request.VariantId, cancellationToken);

            //if(recipe == null || !recipe.Any())
            //{
            //    //không có recipe nào thì ko thể update đc
            //    throw new DirectoryNotFoundException(Message.Recipe.NotFound);
            //}

            var ingredientUpdate = request.Ingredients.Select(i => i.IngredientId).ToList();
            var ingredientInRecipe = recipe.Select(r => r.IngredientId).ToList();

            var addItem = ingredientUpdate.Where(i => !ingredientInRecipe.Contains(i)).ToList();
            var changeItem = ingredientUpdate.Where(i => ingredientInRecipe.Contains(i)).ToList();
            var removeItem = ingredientInRecipe.Where(i => !ingredientUpdate.Contains(i)).ToList();

            //xử lý thêm
            foreach (var ingredientId in addItem)
            {
                var newRecipe = new Domain.Entities.Recipe
                {
                    VariantId = request.VariantId,
                    IngredientId = ingredientId,
                    Quantity = request.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId)?.Quantity ?? 0,
                };
                await _recipeRepository.AddAsync(newRecipe);
            }

            //xử lý sửa
            foreach (var ingredientId in changeItem)
            {
                var recipeToUpdate = recipe.First(r => r.IngredientId == ingredientId);
                recipeToUpdate.Quantity = request.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId)?.Quantity ?? 0;
                await _recipeRepository.UpdateAsync(recipeToUpdate);
            }

            //xử lý xóa
            foreach (var ingredientId in removeItem)
            {
                var recipeToDelete = recipe.First(r => r.IngredientId == ingredientId);
                await _recipeRepository.StrictDelete(recipeToDelete);
            }

            return Unit.Value;
        }
    }
}
