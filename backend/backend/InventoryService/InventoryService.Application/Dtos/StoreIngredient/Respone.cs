using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Ingredient;
using InventoryService.Domain.Entities;
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

    public class StockViewRes
    {
        public Guid StoreId { get; set; } = Guid.Empty;
        public IngredientViewRes Ingredient { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal MinThreshold { get; set; }
        public StockStatus StockStatus { get; set; }
    }


    public class DecreaseStockResult
    {
        public decimal Quantity { get; set; }
        public decimal? MinThreshold { get; set; }
    }

    public sealed class ReleaseStockRes
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
