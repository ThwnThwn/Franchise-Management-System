namespace SalesService.Application.Dtos.Voucher
{
    public partial class VoucherItem
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public VoucherItemType Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
    public enum VoucherItemType { ITEM_GIFT, ITEM_REQUIRED }
}
