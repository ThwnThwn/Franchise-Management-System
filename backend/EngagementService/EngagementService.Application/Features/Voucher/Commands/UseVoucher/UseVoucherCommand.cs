using EngagementService.Application.Dtos.Voucher;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus
{
    public record class UseVoucherCommand(UseVouchersRequest Request) : IRequest<Unit>
    {
    }
}
