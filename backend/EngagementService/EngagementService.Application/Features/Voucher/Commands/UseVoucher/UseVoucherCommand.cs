using EngagementService.Application.Dtos.Voucher;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus
{
    public record class UseVoucherCommand(UseVouchersRequest Request) : IRequest<Unit>
    {
    }
}
