using EngagementService.Application.Dtos.MembershipTier;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.MembershipTier.Commands.Update
{
    public record class UpdateMembershipTierCommand(UpdateMembershipTierRequest Request) : IRequest<Unit>
    {
    }
}
