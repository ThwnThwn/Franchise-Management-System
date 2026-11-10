using Ardalis.Result;
using DocumentFormat.OpenXml.Office2016.Excel;
using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CancelRefund.CancelRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CancelRefund;

public sealed class CancelRefundCommandHandler :
    IRequestHandler<CancelRefundCommand, Result<CancelRefundCommand.CancelRefundResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;

    public CancelRefundCommandHandler(IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<CancelRefundResponse>> Handle(
    CancelRefundCommand command,
    CancellationToken cancellationToken)
    {
        var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(
        command.RefundTicketId,
        cancellationToken);

        if (refundTicket == null)
        {
            return Result.NotFound("Refund ticket not found");
        }

        if (refundTicket.Status != RefundTicketStatus.Pending)
        {
            return Result.Invalid(new ValidationError(
                nameof(refundTicket.Status),
                $"Only pending refund requests can be cancelled. " +
                $"Current status: {refundTicket.Status}"));
        }

        //  CANCEL REFUND TICKET
        refundTicket.Status = RefundTicketStatus.Cancelled;
        refundTicket.CancelledBy = refundTicket.UserId;
        refundTicket.CancelledAt = DateTimeOffset.UtcNow;
        refundTicket.CancellationReason = command.Request?.CancellationReason;
        refundTicket.UpdatedAt = DateTimeOffset.UtcNow;

        var wallet = await _walletRepository.GetByUserIdAsync(refundTicket.UserId, cancellationToken);
        if (wallet == null)
        {
            return Result.Error("Customer wallet not found");
        }

        wallet.Balance += refundTicket.Amount;
        await _walletRepository.UpdateAsync(wallet, cancellationToken);

        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = refundTicket.UserId,
            Amount = +refundTicket.Amount,
            TransactionType = TransactionType.Refund,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = refundTicket.Notes,
            Status = TransactionStatus.Completed,
            TransactionCode = GenerateTransactionCode(TransactionType.Refund)
        };
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        refundTicket.TransactionId = transaction.Id;

        await _refundTicketRepository.UpdateAsync(
            refundTicket,
            cancellationToken);

        var response = new CancelRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Status = refundTicket.Status,
            Amount = refundTicket.Amount,
            CancellationReason = refundTicket.CancellationReason,
            CancelledAt = refundTicket.CancelledAt!.Value,
            Message = "Your refund request has been cancelled successfully."
        };

        return Result.Success(response);
    }


 private static string GenerateTransactionCode(TransactionType type)
    {
        var prefix = type switch
        {
            TransactionType.Deposit => "DEP",
            TransactionType.Withdrawal => "WDR",
            TransactionType.Payment => "PAY",
            TransactionType.Refund => "REF",
            TransactionType.Transfer => "TRF",
            _ => "TXN"
        };

        var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{prefix}-{timestamp}-{shortGuid}";
    }
}