using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Delete
{
    public record class DeleteVoucherCommand(Guid Id) : IRequest<Unit>
    {

    }
}
