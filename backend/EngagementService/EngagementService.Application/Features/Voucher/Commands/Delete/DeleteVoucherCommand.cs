using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.Delete
{
    public record class DeleteVoucherCommand(Guid Id) : IRequest<Unit>
    {

    }
}
