using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.Recipe
{
    public class RecipeViewRes
    {
        public Guid VariantId { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; } = null!;
        public string BaseUnit { get; set; } = null!;
    }
}
