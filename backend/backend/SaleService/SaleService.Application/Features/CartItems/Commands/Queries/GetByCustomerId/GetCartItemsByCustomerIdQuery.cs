using MediatR;
using SalesService.Application.Dtos.CartItem.Response;
using System;
using System.Collections.Generic;

namespace SalesService.Application.Features.CartItems.Queries.GetByCustomerId
{
    public record GetCartItemsByCustomerIdQuery(Guid CustomerId): IRequest<List<CartItemRep>>;
}
