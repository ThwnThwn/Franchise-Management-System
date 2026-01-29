using MediatR;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Dtos.CartItem.Response;

namespace SalesService.Application.Features.CartItems.Commands.Create
{
    public record CreateCartItemCommand(CreateCartItemReq Request) : IRequest<CartItemResponse>;
}
