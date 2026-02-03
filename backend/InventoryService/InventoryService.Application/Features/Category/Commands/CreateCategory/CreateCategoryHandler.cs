using InventoryService.Application.Features.Ingrdient.Commands.CreateIngredient;
using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Unit>
    {
        private readonly IIngredientCategoryRepository _categoryRepository;
        public CreateCategoryHandler(IIngredientCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Unit> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var category = new IngredientCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            await _categoryRepository.AddAsync(category);
            return Unit.Value;
        }
    }
}
