using SharedKernel.Events;

namespace EngagementService.Application.Dtos.LoyaltyAccount
{
    public class UpSertReqest
    {
        public Guid Id { get; set; }
        public decimal OrderAmount { get; set; }
        public Guid OrderId { get; set; } = Guid.Empty;
    }

    public class GetByUserPhoneOrEmailRequest
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class UpdatePointRequest
    {
        public Guid Id { get; set; }
        public decimal Point { get; set; } = 0;
        public Guid? OrderId { get; set; } = null;

        public UpdatePointType ActionType { get; set; }
    }

    public class RedeemVoucherRequest
    {
        public Guid LoyaltyAccountId { get; set; }
        public Guid VoucherId { get; set; }
    }
}