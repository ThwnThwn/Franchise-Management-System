using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.MembershipTier.Commands.Delete
{
    public record class DeleteMembershipTierCommand(Guid Id) : IRequest<Unit>
    {
    }
}
