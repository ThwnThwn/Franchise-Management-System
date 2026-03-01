using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetByEmail
{
    public record class GetUsersByEmailQuery(string? Email) : IRequest<List<User>>
    {
    }
}
