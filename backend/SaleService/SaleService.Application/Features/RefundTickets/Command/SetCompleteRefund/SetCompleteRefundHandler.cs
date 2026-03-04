using MediatR;
using Ardalis.Result;
using SalesService.Application.Persisitences;
namespace SalesService.Application.Features.RefundTickets.Command.SetCompleteRefund;

public sealed class SetCompleteRefundHandler:
    IRequestHandler<SetCompleteRefundCommand, Result<SetCompleteRefundCommand.SetCompleteRefundResponse>>
{
    private readonly IRefundTicketRepository _refundTicketRepository;
    public SetCompleteRefundHandler(IRefundTicketRepository refundTicketRepository)
    {
        _refundTicketRepository = refundTicketRepository;
    }

    public async Task<Result<SetCompleteRefundCommand.SetCompleteRefundResponse>> Handle(
        SetCompleteRefundCommand command,
        CancellationToken cancellationToken)
    {
        var refundTicket = await _refundTicketRepository.GetByTicketIdAsync(command.request.RefundTicketId, cancellationToken);
        if (refundTicket == null)
        {
            return Result.NotFound("Refund ticket not found");
        }
        refundTicket.Status = Domain.Enums.RefundTicketStatus.Completed;
        refundTicket.Notes = command.request.CompleteNotes;
        await _refundTicketRepository.UpdateAsync(refundTicket, cancellationToken);
        var response = new SetCompleteRefundCommand.SetCompleteRefundResponse
        {
            RefundTicketId = refundTicket.Id,
            Status = refundTicket.Status,
            CompleteNotes = command.request.CompleteNotes
        };
        return Result.Success(response);
    }
}