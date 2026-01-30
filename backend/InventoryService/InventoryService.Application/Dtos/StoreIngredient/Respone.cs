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
        public IEnumerable<AmountVariant>? ShortageItems { get; set; }
    }
}
