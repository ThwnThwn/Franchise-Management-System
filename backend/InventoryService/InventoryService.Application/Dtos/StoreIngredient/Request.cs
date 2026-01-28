namespace InventoryService.Application.Dtos.StoreIngredient
{
    public class IncreaseStockReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<IngredientStockItem> Items { get; set; }
    }

    public class IngredientStockItem
    {
        public Guid IngredientId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class DescreaseStockByOrderReq
    {
        public Guid StoreId { get; set; }
        public Guid VariantId { get; set; }
    }

    public class CheckEnoughStockReq
    {
        public Guid StoreId { get; set; }
        public IEnumerable<CheckItem> CheckItem { get; set; } = new List<CheckItem>();
    }

    public class CheckItem
    {
        public Guid VariantId { get; set; }
        public int Amount { get; set; }
    }
}
