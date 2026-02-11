using Grpc.Core;
using InventoryService.Application.Features.StoreIngredient.Commands.CheckEnoughStock;
using InventoryService.Application.Features.StoreIngredient.Commands.DecreaseStockByOrder;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Application.Features.StoreIngredient.Commands.ReleaseStock;
using InventoryService.Grpc.Protos.Inventory.Internal;
using MediatR;

namespace InventoryService.API.Services.Inventory
{
    // Giả sử package trong proto của bạn là: inventory.internal
    public class InventoryInternalService : InventoryInternalGrpc.InventoryInternalGrpcBase
    {
        public readonly IMediator _mediator;
        public InventoryInternalService(IMediator mediator)
        {
            _mediator = mediator;
        }
        public override async Task<CheckEnoughStockResponse> CheckEnoughStock(CheckEnoughStockRequest request, ServerCallContext context)
        {
            //ktra phải manager của kho 

                var rep = await _mediator.Send(new CheckEnoughStockCommand(new ()
                {
                    StoreId = Guid.Parse(request.StoreId),
                    CheckItem = request.CheckItems.Select(x => new Application.Dtos.StoreIngredient.AmountVariant
                    {
                        VariantId = Guid.Parse(x.VariantId),
                        Amount = x.Amount
                    }).ToList()
                }));

                // 1. Khởi tạo đối tượng Response của gRPC
                var response = new CheckEnoughStockResponse
                {
                    IsEnough = rep.IsEnough
                };

                // 2. Chuyển đổi từ DTO sang class ShortageItem của gRPC và thêm vào danh sách
                if (rep.ShortageItems != null)
                {
                    var grpcItems = rep.ShortageItems.Select(x => new AmountVariant
                    {
                        VariantId = x.VariantId.ToString(),
                        Amount = x.Amount
                    });

                    response.ShortageItems.AddRange(grpcItems); // Dùng AddRange thay vì gán dấu "="
                }

                return response;
        }

        public async override Task<DecreaseStockByOrderResponse> DecreaseStockByOrder(DecreaseStockByOrderRequest request, ServerCallContext context)
        {
            var rep = await _mediator.Send(new DecreaseStockByOrderCommand(new()
            {
                StoreId = Guid.Parse(request.StoreId),
                Items = request.DecreaseItems.Select(x => new Application.Dtos.StoreIngredient.AmountVariant
                {
                    VariantId = Guid.Parse(x.VariantId),
                    Amount = x.Amount
                }).ToList()
            }));

            return new DecreaseStockByOrderResponse();
        }

        public override async Task<CheckEnoughStockResponse> ReserveStock(ReserveStockRequest request, ServerCallContext context)
        {
            var command = new Application.Features.StoreIngredient.Commands.ReserveStock.ReserveStockCommand
            {
                StoreId = Guid.Parse(request.StoreId),
                OrderId = Guid.Parse(request.OrderId),
                Items = request.Items.Select(x => new Application.Dtos.StoreIngredient.AmountVariant
                {
                    VariantId = Guid.Parse(x.VariantId),
                    Amount = x.Amount
                }).ToList()
            };

            var rep = await _mediator.Send(command);

            //return new ReserveStockResponse
            //{
            //    Success = result.Success,
            //    Message = result.Message
            //};
            // 1. Khởi tạo đối tượng Response của gRPC
            var response = new CheckEnoughStockResponse
            {
                IsEnough = rep.IsEnough
            };

            // 2. Chuyển đổi từ DTO sang class ShortageItem của gRPC và thêm vào danh sách
            if (rep.ShortageItems != null)
            {
                var grpcItems = rep.ShortageItems.Select(x => new AmountVariant
                {
                    VariantId = x.VariantId.ToString(),
                    Amount = x.Amount
                });

                response.ShortageItems.AddRange(grpcItems); // Dùng AddRange thay vì gán dấu "="
            }

            return response;
        }
        public override async Task<ReleaseStockResponse> ReleaseStock(ReleaseStockRequest request,ServerCallContext context)
        {
            var command = new ReleaseStockCommand(new()
            {
                StoreId = Guid.Parse(request.StoreId),
                OrderId = Guid.Parse(request.OrderId)
            });

            var result = await _mediator.Send(command);

            return new ReleaseStockResponse
            {
                Success = result.Success,
                Message = result.Message
            };
        }
    }
}
