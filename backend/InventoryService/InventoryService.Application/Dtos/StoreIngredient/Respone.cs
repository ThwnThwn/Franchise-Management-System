using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.Dtos.StoreIngredient
{
    public class CheckEnoughStockRes
    {
        public bool IsEnough { get; set; }
        public IEnumerable<ShortageItem>? ShortageItems { get; set; }
    }

    public class ShortageItem
    {
        public Guid VariantId { get; set; }
        public int MaxFeasibleQuantity { get; set; }
    }
}
