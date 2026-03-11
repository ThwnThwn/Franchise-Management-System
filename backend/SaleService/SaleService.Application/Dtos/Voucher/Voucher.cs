namespace SalesService.Application.Dtos.Voucher
{
    public class Voucher
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public VoucherType Type { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? PointCost { get; set; }
        public int? InitQuantity { get; set; }
        public int? CurrentQuantity { get; set; }
        public int? MaxUsagePerUser { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public ICollection<VoucherItem> VoucherItems { get; set; } = new List<VoucherItem>();
    }
    public enum VoucherType
    {
        DISCOUNT_PERCENTAGE,
        DISCOUNT_FIXED_AMOUNT,
        SHIPPING_PERCENTAGE,
        SHIPPING_FIXED_AMOUNT,
        GIFT
    }
}
