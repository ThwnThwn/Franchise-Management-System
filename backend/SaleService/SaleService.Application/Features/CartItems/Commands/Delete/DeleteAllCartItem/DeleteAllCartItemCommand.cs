using MediatR;
using SalesService.Application.Dtos.CartItem.Request;

namespace SalesService.Application.Features.CartItems.Commands.Delete.DeleteAllCartItem
{
    public record DeleteAllCartItemCommand(DeleteAllCartItemReq Request) : IRequest<bool>;
}
