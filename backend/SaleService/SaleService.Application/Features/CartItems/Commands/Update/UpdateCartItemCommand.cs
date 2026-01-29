using MediatR;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Dtos.CartItem.Response;

namespace SalesService.Application.Features.CartItems.Commands.Update
{
    public record UpdateCartItemCommand(UpdateCartItemReq Request): IRequest<CartItemResponse>;
}
