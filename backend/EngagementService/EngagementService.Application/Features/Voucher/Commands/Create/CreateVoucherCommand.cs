using EngagementService.Application.Dtos.Voucher;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.Create
{
    public record class CreateVoucherCommand(CreateVoucherRequest Request) : IRequest<Unit>
    {
    }
}
