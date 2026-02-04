using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.Recipe
{
    public class MutationRecipeReqDto
    {
        public Guid VariantId { get; set; }
        public List<RecipeIngredientRequest> Ingredients { get; set; } = new();
    }
    public class RecipeIngredientRequest
    {
        public Guid IngredientId { get; set; }
        public decimal Quantity { get; set; }
    }
}
