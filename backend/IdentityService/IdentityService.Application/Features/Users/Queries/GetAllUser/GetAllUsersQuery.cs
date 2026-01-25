using IdentityService.Application.Dtos.User.Respone;
using MediatR;

namespace IdentityService.Application.Features.Users.Queries.GetAllUser;

public record GetAllUsersQuery() : IRequest<IEnumerable<UserRep>>;
