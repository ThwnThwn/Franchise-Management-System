using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Application.Persisitences.Wallets;
using SalesService.Application.Persisitences.WalletTransactions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CreateRefund.CreateRefundTicketCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CreateRefund
{
    public sealed class CreateRefundTicketCommandHandler :
        IRequestHandler<CreateRefundTicketCommand, Result<CreateRefundTicketResponse>>
    {
        private readonly IRefundTicketRepository _refundTicketRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _transactionRepository;

        public CreateRefundTicketCommandHandler(
            IRefundTicketRepository refundTicketRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository transactionRepository)
        {
            _refundTicketRepository = refundTicketRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<CreateRefundTicketResponse>> Handle(
            CreateRefundTicketCommand command,
            CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (request.Amount <= 0)
                return Result.Invalid(new ValidationError(
                    nameof(request.Amount),
                    "Amount must be greater than zero"));

            var wallet = await _walletRepository.GetByIdAsync(
                request.WalletId,
                cancellationToken);

            if (wallet == null)
                return Result.NotFound("Wallet not found");

            if (wallet.UserId != command.UserId)
                return Result.Forbidden();

            if (wallet.Status != WalletStatus.Active)
                return Result.Invalid(new ValidationError(
                    nameof(request.WalletId),
                    "Wallet is not active"));

            var originalTransaction = await _transactionRepository.GetByIdAsync(
                request.OriginalTransactionId,
                cancellationToken);

            if (originalTransaction == null)
                return Result.NotFound("Original transaction not found");

            if (originalTransaction.WalletId != request.WalletId)
                return Result.Invalid(new ValidationError(
                    nameof(request.OriginalTransactionId),
                    "Transaction does not belong to this wallet"));

            if (request.Amount > originalTransaction.Amount)
                return Result.Invalid(new ValidationError(
                    nameof(request.Amount),
                    $"Refund amount cannot exceed original transaction amount of {originalTransaction.Amount}"));

            if (await _refundTicketRepository.ExistingRefundWithPendingStatusAsync(wallet.Id,cancellationToken))
                return Result.Invalid(new ValidationError(
                    nameof(request.OriginalTransactionId),
                    "A pending refund already exists for this transaction"));

            // Generate unique ticket code
            var ticketCode = GenerateTicketCode();

            // Create refund ticket entity
            var refundTicket = new RefundTicket
            {
                Id = Guid.NewGuid(),
                TicketCode = ticketCode,
                Amount = request.Amount,
                RefundAmount = request.Amount, // Initial refund amount
                Reason = request.Reason,
                WalletId = request.WalletId,
                UserId = command.UserId,
                OrderId = originalTransaction.OrderId,
                OriginalTransactionId = request.OriginalTransactionId,
                Status = RefundTicketStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            // Save to repository
            var refundTicketId = await _refundTicketRepository.AddAsync(
                refundTicket,
                cancellationToken);

            if (refundTicketId == Guid.Empty)
                return Result.Error("Failed to create refund ticket");

            // Map to response
            var response = new CreateRefundTicketResponse
            {
                Id = refundTicket.Id,
                TicketCode = refundTicket.TicketCode,
                Amount = refundTicket.Amount,
                Reason = refundTicket.Reason,
                WalletId = refundTicket.WalletId,
                OrderId = refundTicket.OrderId,
                TransactionId = refundTicket.TransactionId,
                OriginalTransactionId = refundTicket.OriginalTransactionId,
                CreatedAt = refundTicket.CreatedAt
            };

            return Result.Success(response);
        }

        private static string GenerateTicketCode()
        {
            // Generate unique ticket code: RFT-YYYYMMDD-XXXXX
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Random.Shared.Next(10000, 99999);
            return $"RFT-{timestamp}-{random}";
        }
    }
}