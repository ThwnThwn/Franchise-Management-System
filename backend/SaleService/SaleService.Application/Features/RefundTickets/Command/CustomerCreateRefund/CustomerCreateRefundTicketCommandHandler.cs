using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer.CustomerCreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund.Customer;

public sealed class CustomerCreateRefundTicketCommandHandler
    : IRequestHandler<CustomerCreateRefundTicketCommand, Result<CustomerCreateRefundTicketResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    public CustomerCreateRefundTicketCommandHandler(
        IRefundTicketRepository refundTicketRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository)
    {
        _refundTicketRepository = refundTicketRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<CustomerCreateRefundTicketResponse>> Handle(
        CustomerCreateRefundTicketCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (request.Amount <= 0)
        {
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                "Refund amount must be greater than zero"));
        }

        var refundTicket = new RefundTicket
        {
            Id = Guid.NewGuid(),
            TicketCode = GenerateTicketCode(),
            Amount = request.Amount,
            RefundAmount = request.Amount,
            Reason = request.Reason,
            UserId = command.CustomerId,
            Status = RefundTicketStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

       

        var wallet = await _walletRepository.GetByUserIdAsync(command.CustomerId, cancellationToken);
        if(wallet == null)
        {
            return Result.Error("Customer wallet not found");
        }

        if (wallet.Balance < request.Amount)
            return Result.Invalid(new ValidationError(
                nameof(request.Amount),
                $"Insufficient wallet balance. Requested: {request.Amount:N2}, Available: {wallet.Balance:N2}"));

        wallet.Balance -= request.Amount;
        await _walletRepository.UpdateAsync(wallet, cancellationToken);
        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            UserId = refundTicket.UserId,
            Amount = -refundTicket.Amount,
            TransactionType = TransactionType.Refund,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Notes = refundTicket.Notes,
            TransactionCode = GenerateTransactionCode(TransactionType.Refund)
        };
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        refundTicket.TransactionId = transaction.Id;
        var refundTicketId = await _refundTicketRepository.AddAsync(
           refundTicket,
           cancellationToken);

        if (refundTicketId == Guid.Empty)
        {
            return Result.Error("Failed to create refund request");
        }

        var response = new CustomerCreateRefundTicketResponse
        {
            Id = refundTicket.Id,
            TicketCode = refundTicket.TicketCode,
            Amount = refundTicket.Amount,
            Reason = refundTicket.Reason,
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