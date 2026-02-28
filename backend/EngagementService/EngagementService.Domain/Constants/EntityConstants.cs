namespace EngagementService.Domain.Constants
{
    public enum PointActionType { REDEEM, EARN, REFUND, ADJUST }
    public enum PointReasonCode
    {
        ORDER_COMPLETED,
        BIRTHDAY_GIFT,
        PROMOTION_BONUS,
        REFERRAL_REWARD,
        POINT_EXPIRED,
        POINT_REDEEM,          
        ORDER_CANCEL_REFUND,   
        ORDER_RETURN_DEDUCT,   
        MANUAL_ADJUST          
    }
    public enum VoucherCategory { DISCOUNT_GIFT, SHIPPING }
    public enum VoucherDiscountType { PERCENTAGE, FIXED_AMOUNT, GIFT_ITEM }
    public enum UserVoucherStatus { AVAILABLE, USED, EXPIRED }
    public enum VoucherItemType { GIFT, REQUIRED }
}
