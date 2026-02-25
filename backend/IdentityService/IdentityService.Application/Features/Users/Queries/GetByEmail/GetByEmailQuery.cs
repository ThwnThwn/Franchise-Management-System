using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetByEmail
{
    public record class GetByEmailQuery(string Email) : IRequest<User>
    {
    }
}
