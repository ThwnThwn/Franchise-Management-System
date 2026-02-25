using Ardalis.Result;
using SalesService.Application.Common.Models;
using SalesService.Application.Features.RefundTickets.Command.CreateRefund;

namespace SalesService.Application.Authorization;

public sealed class RefundAuthorizationService : IRefundAuthorizationService
{
    public Result ValidateInternalUserOverridePermissions(
        string userRole,
        InternalUserOverrideInfo InternalUserOverride,
        decimal refundAmount,
        decimal originalAmount,
        DateTimeOffset transactionDate)
    {
        return userRole switch
        {
            "Staff" => ValidateStaffPermissions(InternalUserOverride, refundAmount, originalAmount, transactionDate),
            "Manager" => ValidateManagerPermissions(InternalUserOverride, refundAmount, originalAmount, transactionDate),
            "InternalUser" => ValidateInternalUserPermissions(InternalUserOverride),
            _ => Result.Forbidden($"Role '{userRole}' does not have refund creation permissions")
        };
    }

    public Result<RefundPermissions> GetUserPermissions(string userRole)
    {
        var permissions = userRole switch
        {
            "Cashier" => new RefundPermissions
            {
                CanAutoApprove = false,
                CanBypassTimeLimit = false,
                CanBypassAmountValidation = false,
                MaxRefundAmount = 5_000_000,
                MaxTimeLimitDays = 30,
                MaxOverRefundPercentage = 1.0m
            },
            "Staff" => new RefundPermissions
            {
                CanAutoApprove = true,
                CanBypassTimeLimit = true,
                CanBypassAmountValidation = false,
                MaxRefundAmount = 10_000_000,
                MaxTimeLimitDays = 60,
                MaxOverRefundPercentage = 1.0m
            },
            "Manager" => new RefundPermissions
            {
                CanAutoApprove = true,
                CanBypassTimeLimit = true,
                CanBypassAmountValidation = true,
                MaxRefundAmount = decimal.MaxValue,
                MaxTimeLimitDays = 365,
                MaxOverRefundPercentage = 1.5m
            },
            "InternalUser" => new RefundPermissions
            {
                CanAutoApprove = true,
                CanBypassTimeLimit = true,
                CanBypassAmountValidation = true,
                MaxRefundAmount = decimal.MaxValue,
                MaxTimeLimitDays = int.MaxValue,
                MaxOverRefundPercentage = decimal.MaxValue
            },
            _ => null
        };

        return permissions != null
            ? Result<RefundPermissions>.Success(permissions)
            : Result<RefundPermissions>.Forbidden($"Unknown role: {userRole}");
    }

    
    // ===== STAFF: Auto-approve số tiền nhỏ, bypass time hạn chế =====
    private static Result ValidateStaffPermissions(
        InternalUserOverrideInfo InternalUserOverride,
        decimal refundAmount,
        decimal originalAmount,
        DateTimeOffset transactionDate)
    {
        // ✅ Auto-approve chỉ với số tiền nhỏ
        if (InternalUserOverride.AutoApprove && refundAmount > 1_000_000) // 1 triệu
            return Result.Forbidden($"Staff can only auto-approve refunds up to 1,000,000 VND. " +
                $"Requested: {refundAmount:N0}. Set AutoApprove=false or escalate to Manager.");

        // ✅ Bypass time trong 60 ngày
        if (InternalUserOverride.BypassTimeLimit)
        {
            var daysSinceTransaction = (DateTimeOffset.UtcNow - transactionDate).Days;
            if (daysSinceTransaction > 60)
                return Result.Forbidden($"Staff can only bypass time limit within 60 days. " +
                    $"Transaction is {daysSinceTransaction} days old. Please escalate to Manager.");
        }

        // ❌ Không được bypass amount
        if (InternalUserOverride.BypassAmountValidation)
            return Result.Forbidden("Staff cannot bypass amount validation. Please escalate to Manager.");

        // ✅ Giới hạn tổng số tiền
        if (refundAmount > 10_000_000) // 10 triệu
            return Result.Forbidden($"Staff can only create refunds up to 10,000,000 VND. " +
                $"Requested: {refundAmount:N0}. Please escalate to Manager.");

        return Result.Success();
    }

    // ===== MANAGER: Quyền cao, có thể bypass với giới hạn =====
    private static Result ValidateManagerPermissions(
       InternalUserOverrideInfo InternalUserOverride,
        decimal refundAmount,
        decimal originalAmount,
        DateTimeOffset transactionDate)
    {
        // ✅ Auto-approve mọi số tiền
        // OK

        // ✅ Bypass time trong 1 năm
        if (InternalUserOverride.BypassTimeLimit)
        {
            var daysSinceTransaction = (DateTimeOffset.UtcNow - transactionDate).Days;
            if (daysSinceTransaction > 365)
                return Result.Forbidden($"Managers can only bypass time limit within 1 year. " +
                    $"Transaction is {daysSinceTransaction} days old. Please escalate to InternalUser.");
        }

        // ✅ Bypass amount với giới hạn 150%
        if (InternalUserOverride.BypassAmountValidation)
        {
            var maxAllowed = originalAmount * 1.5m; // 150% of original
            if (refundAmount > maxAllowed)
                return Result.Forbidden($"Managers can only over-refund up to 150% of original amount. " +
                    $"Original: {originalAmount:N0}, Max Allowed: {maxAllowed:N0}, " +
                    $"Requested: {refundAmount:N0}. Please escalate to InternalUser.");

            // ✅ Bắt buộc InternalNotes khi over-refund
            if (string.IsNullOrWhiteSpace(InternalUserOverride.InternalNotes))
                return Result.Invalid(new ValidationError(
                    nameof(InternalUserOverride.InternalNotes),
                    "InternalNotes is required when over-refunding."));
        }

        return Result.Success();
    }

    // ===== InternalUser: Full quyền =====
    private static Result ValidateInternalUserPermissions(
        InternalUserOverrideInfo InternalUserOverride)
    {
        // ✅ Bắt buộc InternalNotes khi bypass
        if ((InternalUserOverride.BypassTimeLimit || InternalUserOverride.BypassAmountValidation) &&
            string.IsNullOrWhiteSpace(InternalUserOverride.InternalNotes))
        {
            return Result.Invalid(new ValidationError(
                nameof(InternalUserOverride.InternalNotes),
                "InternalNotes is required when using bypass features for audit purposes."));
        }

        return Result.Success();
    }
}