using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Sales.Cart;
using SalesService.Application.ClientInterface.Products;
using SalesService.Application.Dtos.CartItem.Request;
using SalesService.Application.Features.CartItems.Commands.Create;
using SalesService.Application.Features.CartItems.Commands.Delete;
using SalesService.Application.Features.CartItems.Commands.Delete.DeleteAllCartItem;
using SalesService.Application.Features.CartItems.Commands.Delete.DeleteListCartItem;
using SalesService.Application.Features.CartItems.Commands.Update;
using SalesService.Application.Features.CartItems.Queries.GetByCustomerId;

namespace SalesService.API.Services.CartItems
{
    public class CartItemGrpcService : CartItemGrpc.CartItemGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IProductServiceClient _productServiceClient;

        public CartItemGrpcService(IMediator mediator, IProductServiceClient productServiceClient)
        {
            _mediator = mediator;
            _productServiceClient = productServiceClient;
        }

        //Add Cart Item
        public override async Task<CartItemReply> AddToCart(AddToCartRequest request, ServerCallContext context)
        {
            var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            if (userIdEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
            }
            var customerId = Guid.Parse(userIdEntry.Value);
            var productVariant = await _productServiceClient.GetProductVariantBySkuAsync(request.Sku);
            if (productVariant == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product variant with SKU '{request.Sku}' not found or inactive."));
            }

            if (!productVariant.BasePrice.HasValue || productVariant.BasePrice <= 0)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Product variant with SKU '{request.Sku}' has invalid price."));
            }
            var command = new CreateCartItemCommand(new CreateCartItemReq
            {
                CustomerId = customerId,
                Sku = request.Sku,
                Quantity = request.Quantity,
                BasePrice = (decimal)productVariant.BasePrice,
            });

            var result = await _mediator.Send(command);

            return new CartItemReply
            {
                Id = result.Id.ToString(),
                CustomerId = result.CustomerId.ToString(),
                Sku = result.Sku,
                Quantity = result.Quantity,
                BasePrice = (double)result.BasePrice
            };
        }

        //Update Quantity
        public override async Task<CartItemReply> UpdateCartItem(UpdateCartItemRequest request, ServerCallContext context)
        {
            var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            if (userIdEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
            }
            var customerId = Guid.Parse(userIdEntry.Value);
            var command = new UpdateCartItemCommand(new UpdateCartItemReq
            {
                CustomerId = customerId,
                Sku = request.Sku,
                Quantity = request.Quantity
            });

            var result = await _mediator.Send(command);

            return new CartItemReply
            {
                Id = result.Id.ToString(),
                CustomerId = result.CustomerId.ToString(),
                Sku = result.Sku,
                ProductName = (await _productServiceClient.GetProductVariantBySkuAsync(result.Sku)).ProductName,
                Quantity = result.Quantity,
                BasePrice = (double)result.BasePrice
            };
        }

        //Delete Single Cart Item
        public override async Task<BoolReply> DeleteListCartItem(DeleteListCartItemRequest request, ServerCallContext context)
        {
            var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            if (userIdEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
            }
            var customerId = Guid.Parse(userIdEntry.Value);

            var command = new DeleteListCartItemCommand(
                new DeleteListCartItemReq()
                {
                    CustomerId = customerId,
                    Skus = request.Skus.ToList()
                }
            );

            var success = await _mediator.Send(command);

            return new BoolReply { Success = success };
        }

        //Get All Items in Cart
        public override async Task<CartItemListReply> GetCartItems(Empty request, ServerCallContext context)
        {
            var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            if (userIdEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
            }

            var customerId = Guid.Parse(userIdEntry.Value);
            var query = new GetCartItemsByCustomerIdQuery(
                customerId
            );

            var items = await _mediator.Send(query);

            var reply = new CartItemListReply();

            reply.Items.AddRange(items.Select(x => new CartItemReply
            {
                Id = x.Id.ToString(),
                CustomerId = customerId.ToString(),
                Sku = x.Sku,
                ProductName = x.ProductName,
                Attributes = x.Attributes,
                ImageUrl = x.ImageUrl,
                Quantity = x.Quantity,
                BasePrice = (double)x.BasePrice,

                CreatedAt = x.CreatedAt.ToString("o"),
                UpdatedAt = x.UpdatedAt.ToString("o")
            }));

            return reply;
        }

        //Clear Cart
        public override async Task<BoolReply> ClearCart(Empty request, ServerCallContext context)
        {
            var userIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            if (userIdEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing x-user-id in request headers"));
            }
            var customerId = Guid.Parse(userIdEntry.Value);

            var command = new DeleteAllCartItemCommand(
                new DeleteAllCartItemReq
                {
                    CustomerId = customerId
                }
            );

            var success = await _mediator.Send(command);
            return new BoolReply { Success = success };
        }
    }
}