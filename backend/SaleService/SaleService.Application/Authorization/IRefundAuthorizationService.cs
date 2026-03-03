using Ardalis.Result;
using SalesService.Application.Common.Models;

namespace SalesService.Application.Authorization;

public interface IRefundAuthorizationService
{
    Result ValidateInternalUserOverridePermissions(
        string userRole,
        InternalUserOverrideInfo InternalUserOverride,
        decimal refundAmount,
        decimal originalAmount,
        DateTimeOffset transactionDate);

    Result<RefundPermissions> GetUserPermissions(string userRole);
}

public class RefundPermissions
{
    public bool CanAutoApprove { get; set; }
    public bool CanBypassTimeLimit { get; set; }
    public bool CanBypassAmountValidation { get; set; }
    public decimal MaxRefundAmount { get; set; }
    public int MaxTimeLimitDays { get; set; }
    public decimal MaxOverRefundPercentage { get; set; }
}