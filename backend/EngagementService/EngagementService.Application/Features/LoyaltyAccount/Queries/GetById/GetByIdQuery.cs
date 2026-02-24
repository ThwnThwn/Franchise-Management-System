using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.LoyaltyAccount.Queries.GetById
{
    public record class GetByIdQuery(Guid Id) : IRequest<Domain.Entities.LoyaltyAccount?>
    {
    }
}
