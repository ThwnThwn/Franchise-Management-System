using InventoryService.Application.Constants;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly IIngredientCategoryRepository _categoryRepository;
        public UpdateCategoryHandler(IIngredientCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<Unit> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var categoryInDb =  _categoryRepository.GetByIdAsync(request.Id).Result
                ?? throw new DirectoryNotFoundException(Message.Category.NotFound);

            if(!string.IsNullOrEmpty(request.Name))
                categoryInDb.Name = request.Name;
            if(!string.IsNullOrEmpty(request.Description))
                categoryInDb.Description = request.Description;
            await _categoryRepository.UpdateAsync(categoryInDb);
            return Unit.Value;
        }
    }
}
