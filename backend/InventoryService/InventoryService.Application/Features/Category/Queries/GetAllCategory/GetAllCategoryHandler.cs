using InventoryService.Application.Dtos.Category;
using InventoryService.Application.Persistences.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Features.Category.Queries.GetAllCategory
{
    public class GetAllCategoryHandler : IRequestHandler<GetAllCategoryQuery, List<CategoryViewRes>>
    {
        private readonly IIngredientCategoryRepository _categoryRepository;
        public GetAllCategoryHandler(IIngredientCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<List<CategoryViewRes>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
        {
            var categoryLists =  await _categoryRepository.GetAllAsync();
            var result = categoryLists.Select(c => new CategoryViewRes
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
            return result;
        }
    }
}
