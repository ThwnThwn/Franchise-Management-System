using EngagementService.Application.Dtos.Voucher;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Update
{
    public record class UpdateVoucherCommand(UpdateVoucherRequest Request) : IRequest<Unit>
    {
    }
}
