using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.Ingredient
{
    public class IngredientViewRes
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string BaseUnit { get; set; } = null!;

        public Guid? CategoryId { get; set; }
    }
}
