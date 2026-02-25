using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Features.Users.Queries.GetByPhone
{
    public record class GetUserByPhoneQuery(string PhoneNumber) : IRequest<Domain.Entities.User>
    {
    }
}
