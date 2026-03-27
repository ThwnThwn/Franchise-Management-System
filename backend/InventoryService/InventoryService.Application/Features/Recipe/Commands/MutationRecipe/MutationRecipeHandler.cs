using InventoryService.Application.Abstractions;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IInventoryDbContext _dbContext;

        public MutationRecipeHandler(IRecipeRepository recipeRepository, IProductServiceClient productServiceClient, IInventoryDbContext dbContext)
        {
            _recipeRepository = recipeRepository;
            _productServiceClient = productServiceClient;
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(MutationRecipeCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var variant = await _productServiceClient.GetVariantById(request.VariantId)
                ?? throw new DirectoryNotFoundException("Variant not found");

            var recipe = await _recipeRepository.GetRecipesByVariantIdAsync(request.VariantId, cancellationToken);

            var ingredientUpdate = request.Ingredients.Select(i => i.IngredientId).ToList();
            var ingredientInRecipe = recipe.Select(r => r.IngredientId).ToList();

            var addItem = ingredientUpdate.Where(i => !ingredientInRecipe.Contains(i)).ToList();
            var changeItem = ingredientUpdate.Where(i => ingredientInRecipe.Contains(i)).ToList();
            var removeItem = ingredientInRecipe.Where(i => !ingredientUpdate.Contains(i)).ToList();

            // 1. Xử lý thêm
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

            // 2. Xử lý sửa
            foreach (var ingredientId in changeItem)
            {
                var recipeToUpdate = recipe.First(r => r.IngredientId == ingredientId);
                recipeToUpdate.Quantity = request.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId)?.Quantity ?? 0;
                await _recipeRepository.UpdateAsync(recipeToUpdate);
            }

            // 3. Xử lý xóa (DÙNG TRANSACTION QUA INTERFACE ĐỂ TEST ĐƯỢC)
            using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var ingredientId in removeItem)
                {
                    var recipeToDelete = recipe.First(r => r.IngredientId == ingredientId);
                    await _recipeRepository.DeleteAsync(recipeToDelete.Id);
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return Unit.Value;
        }
    }
}