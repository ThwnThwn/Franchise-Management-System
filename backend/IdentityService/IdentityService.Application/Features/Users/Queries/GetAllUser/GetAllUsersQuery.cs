using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public record GetAllUsersQuery(string RoleSlug) : IRequest<IEnumerable<UserRep>>;
