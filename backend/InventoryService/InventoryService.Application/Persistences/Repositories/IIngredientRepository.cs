using BuildingBlocks.Core.Pagination;
using InventoryService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Persistences.Repositories
{
    public interface IIngredientRepository : IGenericRepository<Ingredient>
    {
        Task<PagedResult<Ingredient>> GetPagedIngredientsAsync(Guid? categoryId, int pageIndex, int pageSize, CancellationToken cancellationToken);
    }
}
