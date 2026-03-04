namespace EngagementService.Domain.Constants
{
    public enum PointActionType { REDEEM, EARN, REFUND, ADJUST
            //, YEARLY_ROLLOVER, PERIODIC_EXPIRATION
    }
    public enum PointReasonCode
    {
        ORDER_COMPLETED,
        BIRTHDAY_GIFT,
        PROMOTION_BONUS,
        REFERRAL_REWARD,
        POINT_EXPIRED,
        POINT_REDEEM,
        //YEARLY_ROLLOVER,
        //PERIODIC_EXPIRATION,
        ORDER_CANCEL_REFUND,   
        ORDER_RETURN_DEDUCT,   
        MANUAL_ADJUST          
    }
<<<<<<< HEAD
    public enum VoucherCategory { DISCOUNT, SHIPPING, GIFT }
    public enum VoucherDiscountType { PERCENTAGE, FIXED_AMOUNT, GIFT_ITEM }
=======
    public enum VoucherType { DISCOUNT_PERCENTAGE,
                                DISCOUNT_FIXED_AMOUNT,
                                SHIPPING_PERCENTAGE,
                                SHIPPING_FIXED_AMOUNT,
                                GIFT
    }
    //public enum VoucherDiscountType { PERCENTAGE, FIXED_AMOUNT, GIFT_ITEM }
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
    public enum UserVoucherStatus { AVAILABLE, USED, EXPIRED }
    public enum VoucherItemType { ITEM_GIFT, ITEM_REQUIRED }
}
