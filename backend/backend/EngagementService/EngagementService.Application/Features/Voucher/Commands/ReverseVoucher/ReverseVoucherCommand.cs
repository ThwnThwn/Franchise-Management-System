using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.ReverseVoucher
{
    public record class ReverseVoucherCommand(Guid OrderId) : IRequest<Unit>
    {
    }
}
