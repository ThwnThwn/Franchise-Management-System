using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.ReverseVoucher
{
    public record class ReverseVoucherCommand(Guid OrderId) : IRequest<Unit>
    {
    }
}
