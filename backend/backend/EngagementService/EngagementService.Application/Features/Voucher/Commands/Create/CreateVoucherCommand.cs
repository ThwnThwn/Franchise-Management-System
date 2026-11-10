using EngagementService.Application.Dtos.Voucher;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Create
{
    public record class CreateVoucherCommand(CreateVoucherRequest Request) : IRequest<Domain.Entities.Voucher>
    {
    }
}