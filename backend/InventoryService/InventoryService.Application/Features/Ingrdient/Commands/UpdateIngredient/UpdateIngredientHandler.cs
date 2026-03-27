using AutoMapper;
using InventoryService.Application.Constants;
using InventoryService.Application.Helper;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Commands.UpdateIngredient
{
    public class UpdateIngredientHandler : IRequestHandler<UpdateIngredientCommand, Unit>
    {
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;
        public UpdateIngredientHandler(IIngredientRepository ingredientRepository, IIngredientCategoryRepository ingredientCategoryRepository)
        {
            _ingredientRepository = ingredientRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
        }
        public async Task<Unit> Handle(UpdateIngredientCommand command, CancellationToken cancellationToken)
        {
            if(command.Request.CategoryId != null &&(await _ingredientCategoryRepository.GetByIdAsync((Guid)command.Request.CategoryId) == null))
                throw new DirectoryNotFoundException(Message.Category.NotFound);
            var itemInDb = await _ingredientRepository.GetByIdAsync(command.Request.Id);
            if (itemInDb == null) throw new DirectoryNotFoundException(Message.Ingredient.NotFound);
            
            if(command.Request.Name != null) itemInDb.Name = command.Request.Name;
            if(command.Request.CategoryId != null) itemInDb.CategoryId = command.Request.CategoryId;
            if(!String.IsNullOrEmpty(command.Request.BaseUnit) && CheckValidBaseUnit.IsValid(command.Request.BaseUnit)) itemInDb.BaseUnit = command.Request.BaseUnit;
            await _ingredientRepository.UpdateAsync(itemInDb);
            return Unit.Value;
        }
    }
}
