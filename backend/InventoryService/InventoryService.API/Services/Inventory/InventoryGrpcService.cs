using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Features.StoreIngredient.Commands;
using InventoryService.Application.Features.StoreIngredient.Commands.IncreaseStock;
using InventoryService.Grpc.Protos.Ingredient;
using InventoryService.Grpc.Protos.Inventory;
using MediatR;
using Sprache;

namespace InventoryService.API.Services.Inventory
{
    public class InventoryGrpcService : InventoryGrpc.InventoryGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IIdentityServiceClient _identityServiceClient;
        public InventoryGrpcService(IMediator mediator, IIdentityServiceClient identityServiceClient)
        {
            _mediator = mediator;
            _identityServiceClient = identityServiceClient;
        }

        public override async Task<IncreaseStockResponse> IncreaseStock(IncreaseStockRequest request, ServerCallContext context)
        {
            //ktra phải manager của kho 
            var managerIdEntry = context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            var manager = await _identityServiceClient.GetManager(new Application.Dtos.User.GetManagerReq() { StoreId = Guid.Parse(request.StoreId)});
            if(manager.Id != Guid.Parse(managerIdEntry.Value))
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Bạn không có quyền thực hiện hành động này"));
            }
            //===============================
            var rep = await _mediator.Send(new IncreaseStockCommand(new()
            {
                StoreId = Guid.Parse(request.StoreId),
                Items = request.Items.Select(x => new Application.Dtos.StoreIngredient.IngredientStockItem
                {
                    IngredientId = Guid.Parse(x.IngredientId),
                    Quantity = Convert.ToDecimal(x.Quantity),
                    UnitPrice = Convert.ToDecimal(x.UnitPrice)
                }).ToList()
            }));
            return new IncreaseStockResponse();
        }

        public override async Task<GetAllStockRep> GetAllStock(GetAllStockReq request, ServerCallContext context)
        {
            IEnumerable<Guid> storeIds;
            IEnumerable<Guid> categoryIds;
            IEnumerable<Guid> ingredientIds;
            try
            {
                storeIds = (request.StoreIds != null && request.StoreIds.Any()) ? request.StoreIds.Select(i => Guid.Parse(i)) : new List<Guid>();
                categoryIds = (request.CategoryIds != null && request.CategoryIds.Any()) ? request.CategoryIds.Select(i => Guid.Parse(i)) : new List<Guid>();
                ingredientIds = (request.IngredientIds != null && request.IngredientIds.Any()) ? request.IngredientIds.Select(i => Guid.Parse(i)) : new List<Guid>();
            }
            catch (ParseException ex)
            {
                throw new ParseException("Lỗi định dạng dữ liệu đầu vào");
            }

            var res = await _mediator.Send(new Application.Features.StoreIngredient.Queries.GetAllStock.GetAllStockQuery()
            {
                StoreIds = storeIds,
                CategoryIds = categoryIds,
                StockStatus = (StockStatus)request.StockStatus,
                IngredientIds = ingredientIds,
                Page = request.Page,
                PageSize = request.PageSize
            });

            var response = new GetAllStockRep()
            {
                TotalPages = res.TotalPages,
                TotalCount = ((int)res.TotalCount)
            };

            // 2. Map thủ công từng item sang Proto message
            var protoStockList = res.Items.Select(s => new StockViewRes
            {
                StoreId = s.StoreId.ToString(),
                Quantity = s.Quantity.ToString(),
                MinThreshold = s.MinThreshold.ToString(),
                StockStatus = (int)s.StockStatus,

                // Map object lồng IngredientViewRes
                IngredientViewRes = new Grpc.Protos.Ingredient.IngredientViewRes
                {
                    Id = s.Ingredient.Id.ToString(),
                    Name = s.Ingredient.Name,
                    BaseUnit = s.Ingredient.BaseUnit,
                    CategoryId = s.Ingredient.CategoryId == null ? string.Empty : s.Ingredient.CategoryId.ToString()
                }
            });

            // 3. Add danh sách đã map vào repeated field
            response.StockRes.AddRange(protoStockList);
            return response;
        }
    }
}
