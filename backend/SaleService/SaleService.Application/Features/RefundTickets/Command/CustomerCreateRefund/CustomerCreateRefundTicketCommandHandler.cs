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

        Console.WriteLine("wallet: "+refundTicket.WalletId);
        Console.WriteLine("or walllet: "+refundTicket.OriginalTransactionId);

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
}