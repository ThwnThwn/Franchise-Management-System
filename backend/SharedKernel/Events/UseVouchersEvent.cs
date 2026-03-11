namespace SharedKernel.Events
{
    public class UseVouchersEvent
    {
        public List<Guid> VoucherIds { get; set; } = new List<Guid>();
        public Guid OrderId { get; set; } // dùng để tạo voucher usage
        public Guid UserId { get; set; } // dùng để tạo voucher usage
    }
}
