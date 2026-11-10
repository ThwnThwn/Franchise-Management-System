using EngagementService.Application.Dtos.Voucher;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus
{
    public record class UpdateVoucherStatusCommand(UpdateVoucherStatusRequest Request) : IRequest<Unit>
    {
    }
}
