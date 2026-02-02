using AutoMapper;
using InventoryService.Application.Constants;
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
        private readonly IMapper _mapper;
        public UpdateIngredientHandler(IIngredientRepository ingredientRepository, IMapper mapper)
        {
            _ingredientRepository = ingredientRepository;
            _mapper = mapper;
        }
        public async Task<Unit> Handle(UpdateIngredientCommand command, CancellationToken cancellationToken)
        {
            if ((await _ingredientRepository.GetByIdAsync(command.Request.Id)) == null)
                throw new DirectoryNotFoundException(Message.Ingredient.NotFound);

            await _ingredientRepository.UpdateAsync(_mapper.Map<Domain.Entities.Ingredient>(command.Request));
            return Unit.Value;
        }
    }
}
