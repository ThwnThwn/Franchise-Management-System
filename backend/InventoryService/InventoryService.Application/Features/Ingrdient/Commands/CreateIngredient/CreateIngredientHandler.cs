using InventoryService.Application.AppExceptions;
using InventoryService.Application.Constants;
using InventoryService.Application.Helper;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient
{
    public class CreateIngredientHandler : IRequestHandler<CreateIngredientCommand, Unit>
    {
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;
        
        public CreateIngredientHandler(IIngredientRepository ingredientRepository, IIngredientCategoryRepository ingredientCategoryRepository)
        {
            _ingredientRepository = ingredientRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
        }
        public async Task<Unit> Handle(CreateIngredientCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            
            if(request.CategoryId != null)
            {
                if ((await _ingredientCategoryRepository.GetByIdAsync((Guid)request.CategoryId)) == null)
                    throw new DirectoryNotFoundException(Message.Category.NotFound);
            }

            //if (!CheckValidBaseUnit.IsValid(request.BaseUnit)) 
            //    throw new BusinessException(Message.Ingredient.InvalidBaseUnit);

            var newIngredient = new Ingredient
            {
                Name = request.Name,
                BaseUnit = request.BaseUnit,
                CategoryId = request.CategoryId
            };

            await _ingredientRepository.AddAsync(newIngredient);

            return Unit.Value;
        }

        
    }
}
