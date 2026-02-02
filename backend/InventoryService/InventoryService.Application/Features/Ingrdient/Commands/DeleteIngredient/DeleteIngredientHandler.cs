using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Ingrdient.Commands.DeleteIngredient
{
    public class DeleteIngredientHandler : IRequestHandler<DeleteIngredientCommnad, Unit>
    {
        private readonly IIngredientRepository _ingredientRepository;
        public DeleteIngredientHandler(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }
        public async Task<Unit> Handle(DeleteIngredientCommnad command, CancellationToken cancellationToken)
        {
            var IngredientId = command.Request;
            await _ingredientRepository.DeleteAsync(IngredientId);
            return Unit.Value;
        }
    }
}   
