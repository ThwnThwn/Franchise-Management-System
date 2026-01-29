using MediatR;
using SalesService.Application.Dtos.CartItem.Request;

namespace SalesService.Application.Features.CartItems.Commands.Delete.DeleteListCartItem
{
    public record DeleteListCartItemCommand(DeleteListCartItemReq Request): IRequest<bool>;
}
