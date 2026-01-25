using MediatR;

namespace IdentityService.Application.Features.Users.Commands.Delete;

public record DeleteUserCommand(Guid Id) : IRequest<bool>;
