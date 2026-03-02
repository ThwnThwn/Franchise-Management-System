using EngagementService.Application.Dtos.LoyaltyAccount;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public record class GetByPhoneOrEmailQuery(GetByUserPhoneOrEmailRequest Request) : IRequest<List<LoyaltyAccountDto>>
    {
    }
}
