using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Queries.GetByCode
{
    public record class GetVoucherByCodeQuery(string code) : IRequest<Domain.Entities.Voucher>
    {
    }
}
