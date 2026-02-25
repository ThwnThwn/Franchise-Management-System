namespace EngagementService.Domain.Constants
{
    public enum PointActionType { REDEEM, EARN, REFUND, ADJUST }
    public enum PointReasonCode { ORDER_PAYMENT, BIRTHDAY, CANCEL_ORDER, PROMOTION, MANUAL_ADJUST }
    public enum VoucherCategory { DISCOUNT_GIFT, SHIPPING }
    public enum VoucherDiscountType { PERCENTAGE, FIXED_AMOUNT, GIFT_ITEM }
    public enum UserVoucherStatus { AVAILABLE, USED, EXPIRED }
    public enum VoucherItemType { GIFT, REQUIRED }
}
