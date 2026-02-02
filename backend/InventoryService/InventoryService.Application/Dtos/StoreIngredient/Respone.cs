using InventoryService.Application.Constants;
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

    public class GetAllStockViewRes
    {
        public Guid StoreId { get; set; } = Guid.Empty;
        public Guid IngredientId { get; set; }
        public string IngredientName { get; set; } = null!;
        public string BaseUnit { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal MinThreshold { get; set; }
        public StockStatus StockStatus { get; set; }
    }
}
