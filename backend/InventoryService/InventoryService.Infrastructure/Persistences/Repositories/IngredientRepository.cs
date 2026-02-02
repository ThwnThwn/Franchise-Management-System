using InventoryService.Application.Persistences.Repositories;
using InventoryService.Domain.Entities;
using InventoryService.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Persistences.Repositories
{
    public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(IInventoryDbContext dbContext) : base(dbContext)
        {
        }
    }
}
