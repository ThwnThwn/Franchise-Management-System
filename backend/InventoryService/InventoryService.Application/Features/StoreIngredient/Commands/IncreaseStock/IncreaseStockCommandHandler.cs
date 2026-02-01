using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.StoreIngredient;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock
{
    public class IncreaseStockCommandHandler : IRequestHandler<IncreaseStockCommand, Unit>
    {
        private readonly IStoreIngredientRepository _storeIngredientRepo;
        public IncreaseStockCommandHandler(IStoreIngredientRepository storeIngredientRepo)
        {
            _storeIngredientRepo = storeIngredientRepo;
        }

        public async Task<Unit> Handle(IncreaseStockCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            if(request.Items == null || !request.Items.Any())
                throw new AppExceptions.BusinessException(Message.Ingredient.Required);
            await _storeIngredientRepo.IncreaseStockAsync(request.StoreId, request.Items);
            return Unit.Value;
        }

    }
}
