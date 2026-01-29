using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InventoryService.Application.Features.StoreIngredient.Commands;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Grpc.Protos.Inventory;
using MediatR;

namespace InventoryService.API.Services
{
    public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
    {
        private readonly IMediator _mediator;
        public InventoryGrpcService(IMediator mediator) { 
            _mediator = mediator;
        }

        public override async Task<IncreaseStockResponse> IncreaseStock(IncreaseStockRequest request, ServerCallContext context)
        {
            //ktra phải manager của kho 
            var managerIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
            //===============================
            var rep = await _mediator.Send(new IncreaseStockCommand(new()
                {
                    StoreId = Guid.Parse(request.StoreId),
                    Items = request.Items.Select(x => new Application.Dtos.StoreIngredient.IngredientStockItem
                    {
                        IngredientId = Guid.Parse(x.IngredientId),
                        Quantity = Convert.ToDecimal(x.Quantity)
                    }).ToList()
                }));
                return new IncreaseStockResponse();   
        }

    }
}
