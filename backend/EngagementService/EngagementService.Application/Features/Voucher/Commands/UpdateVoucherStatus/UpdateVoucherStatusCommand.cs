using EngagementService.Application.Dtos.Voucher;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus
{
    public record class UpdateVoucherStatusCommand(UpdateVoucherStatusRequest Request) : IRequest<Unit>
    {
    }
}
