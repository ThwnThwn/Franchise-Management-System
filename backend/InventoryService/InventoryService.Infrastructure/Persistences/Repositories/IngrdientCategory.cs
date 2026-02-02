using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Persistences.Repositories
{
    public class IngredientCategoryRepository : GenericRepository<IngredientCategory>, IIngredientCategoryRepository
    {
        public IngredientCategoryRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }
    }
}
