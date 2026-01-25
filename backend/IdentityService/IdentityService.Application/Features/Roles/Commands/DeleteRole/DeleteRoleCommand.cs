using MediatR;

namespace IdentityService.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;
