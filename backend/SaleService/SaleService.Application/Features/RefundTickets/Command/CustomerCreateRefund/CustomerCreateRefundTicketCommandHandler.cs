using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;

public sealed class CustomerCreateRefundTicketCommandHandler
    : IRequestHandler<CustomerCreateRefundTicketCommand, Result<CustomerCreateRefundTicketResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public CustomerCreateRefundTicketCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _refundTicketRepository = refundTicketRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<CustomerCreateRefundTicketResponse>> Handle(
        CustomerCreateRefundTicketCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // ===== STEP 1: VALIDATE AMOUNT =====
        if (request.Amount <= 0)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                "Refund amount must be greater than zero"));
        }

        // ===== STEP 2: FIND PAYMENT TRANSACTION =====
        var originalTransaction = await _transactionRepository.GetByOrderIdSingleAsync(
            command.OrderId,
            cancellationToken);

        // ✅ Check null FIRST
        if (originalTransaction == null)
        {
            return Result.NotFound("No payment transaction found for this order");
        }

        // ===== STEP 3: VALIDATE TRANSACTION TYPE =====
        if (originalTransaction.TransactionType != TransactionType.Payment)
        {
            return Result.Invalid(new ValidationError(
                "TransactionType",
                "Only payment transactions can be refunded"));
        }

        // ===== STEP 4: VALIDATE TRANSACTION STATUS =====
        if (originalTransaction.Status != TransactionStatus.Completed)
        {
            return Result.Invalid(new ValidationError(
                "TransactionStatus",
                $"Only completed transactions can be refunded. " +
                $"Current status: {originalTransaction.Status}"));
        }

        // ===== STEP 5: VALIDATE OWNERSHIP =====
        if (originalTransaction.UserId != command.CustomerId)
        {
            return Result.Forbidden("This order does not belong to you");
        }

        // ===== STEP 6: VALIDATE REFUND TIME LIMIT =====
        var refundDeadline = originalTransaction.CreatedAt.AddDays(30);
        if (DateTimeOffset.UtcNow > refundDeadline)
        {
            return Result.Invalid(new ValidationError(
                "RefundDeadline",
                $"Refund window has expired. " +
                $"Deadline was {refundDeadline:yyyy-MM-dd}"));
        }

        // ===== STEP 7: CHECK PENDING REFUND =====
        var hasPendingRefund = await _refundTicketRepository
            .ExistingRefundWithPendingStatusAsync(
                originalTransaction.Id,
                cancellationToken);

        if (hasPendingRefund)
        {
            return Result.Invalid(new ValidationError(
                "PendingRefund",
                "A refund request is already pending for this order. " +
                "Please wait for it to be processed."));
        }

        // ===== STEP 8: VALIDATE REFUNDABLE AMOUNT =====
        var totalRefunded = await _refundTicketRepository
            .GetTotalRefundedAmountByTransactionIdAsync(
                originalTransaction.Id,
                cancellationToken);

        var remainingRefundable = originalTransaction.Amount - totalRefunded;

        if (request.Amount > remainingRefundable)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                $"Refund amount exceeds remaining refundable amount. " +
                $"Original: {originalTransaction.Amount:N2}, " +
                $"Already refunded: {totalRefunded:N2}, " +
                $"Remaining: {remainingRefundable:N2}"));
        }

        // ===== STEP 9: CREATE REFUND TICKET =====
        var refundTicket = new RefundTicket
        {
            Id = Guid.NewGuid(),
            TicketCode = GenerateTicketCode(),
            Amount = request.Amount,
            RefundAmount = request.Amount, // Same as Amount initially
            Reason = request.Reason,
            WalletId = originalTransaction.WalletId,
            UserId = command.CustomerId,
            OrderId = command.OrderId,
            OriginalTransactionId = originalTransaction.Id,
            Status = RefundTicketStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // ===== STEP 10: SAVE TO DATABASE =====
        var refundTicketId = await _refundTicketRepository.AddAsync(
            refundTicket,
            cancellationToken);

        if (refundTicketId == Guid.Empty)
        {
            return Result.Error("Failed to create refund request");
        }

        // ===== STEP 11: RETURN RESPONSE =====
        var response = new CustomerCreateRefundTicketResponse
        {
            Id = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Amount = refundTicket.Amount,
            Reason = refundTicket.Reason,
            OrderId = command.OrderId,
            Status = RefundTicketStatus.Pending, 
            CreatedAt = refundTicket.CreatedAt
        };

        return Result.Success(response);
    }

    private static string GenerateTicketCode()
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"RFT-CUST-{timestamp}-{shortGuid}"; // ✅ CUST prefix for customer-created
    }
}