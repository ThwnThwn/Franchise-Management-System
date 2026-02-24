using IdentityService.Application.Dtos.User.Respone;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Users.Queries.GetUserByIds
{
    public record GetUserByIdsQuery(IReadOnlyList<Guid> UserIds) : IRequest<List<UserRep?>>;
}
