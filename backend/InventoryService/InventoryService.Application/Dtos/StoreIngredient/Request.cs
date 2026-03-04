namespace InventoryService.Application.Dtos.StoreIngredient
{
    public class IncreaseStockReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<IngredientStockItem> Items { get; set; } = [];
    }

    public class IngredientStockItem
    {
        public Guid IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class DecreaseStockByOrderReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<AmountVariant> Items { get; set; } = [];
    }

    public class CheckEnoughStockReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<AmountVariant> CheckItem { get; set; } = new List<AmountVariant>();
    }

    public class AmountVariant
    {
        public Guid VariantId { get; set; }
        public string Sku { get; set; } = default!;
        public int Amount { get; set; }
    }

    public class ReleaseStockReq
    {
        public Guid StoreId { get; set; }
        public Guid OrderId { get; set; }
    }
}