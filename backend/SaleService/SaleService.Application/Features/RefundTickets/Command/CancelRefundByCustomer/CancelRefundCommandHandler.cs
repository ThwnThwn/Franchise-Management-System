using Ardalis.Result;
using MediatR;
using SalesService.Application.Persisitences;
using SalesService.Domain.Enums;
using static SalesService.Application.Features.RefundTickets.Command.CancelRefund.CancelRefundCommand;

namespace SalesService.Application.Features.RefundTickets.Command.CancelRefund
{
    internal sealed class CancelRefundCommandHandler :
        IRequestHandler<CancelRefundCommand, Result<CancelRefundCommand.CancelRefundResponse>>
    {
        private readonly IRefundTicketRepository _refundTicketRepository;

        public CancelRefundCommandHandler(IRefundTicketRepository refundTicketRepository)
        {
            _refundTicketRepository = refundTicketRepository;
        }

        public async Task<Result<CancelRefundResponse>> Handle(
        CancelRefundCommand command,
        CancellationToken cancellationToken)
        {
            // ===== STEP 1: LOAD REFUND TICKET =====
            var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(
            command.RefundTicketId,
            cancellationToken);

            if (refundTicket == null)
            {
                return Result.NotFound("Refund ticket not found");
            }

            if (refundTicket.UserId != command.CustomerId)
            {
                return Result.NotFound("Your are not the owner!");
            }

            // ===== STEP 3: VALIDATE STATUS - Only Pending Can Be Cancelled =====
            if (refundTicket.Status != RefundTicketStatus.Pending)
            {
                return Result.Invalid(new ValidationError(
                    nameof(refundTicket.Status),
                    $"Only pending refund requests can be cancelled. " +
                    $"Current status: {refundTicket.Status}"));
            }

            // ===== STEP 4: CANCEL REFUND TICKET =====
            refundTicket.Status = RefundTicketStatus.Cancelled;
            refundTicket.CancelledBy = command.CustomerId;
            refundTicket.CancelledAt = DateTime.UtcNow;
            refundTicket.CancellationReason = command.Request?.CancellationReason;
            refundTicket.UpdatedAt = DateTimeOffset.UtcNow;

            // ===== STEP 5: SAVE CHANGES =====
            await _refundTicketRepository.UpdateAsync(
                refundTicket,
                cancellationToken);

            // ===== STEP 6: RETURN RESPONSE =====
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
    }
}
