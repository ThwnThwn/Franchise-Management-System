using Grpc.Net.Client;
using InventoryService.Grpc.Protos.Inventory.Internal;
using MediatR;
using SalesService.Application.ClientInterface.Inventory;
using SalesService.Application.Constants;
using SalesService.Application.Dtos.Inventory;

namespace SalesService.API.Services.Inventory
{
    public class InventoryServiceClient : IInventoryServiceClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _inventoryServiceUrl;
        private readonly InventoryInternalGrpc.InventoryInternalGrpcClient _client;

        public InventoryServiceClient(IMediator mediator, IConfiguration configuration)
        {
            _configuration = configuration;

            var rawUrl = configuration[Common.INVENTORY_SERVICE_URL] ?? "https://localhost:5005";
            // Allow env var values like "localhost:5002" by normalizing to an absolute URI.
            _inventoryServiceUrl = rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                 rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? rawUrl
                : $"https://{rawUrl}";

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(_inventoryServiceUrl, new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            _client = new InventoryInternalGrpc.InventoryInternalGrpcClient(channel);
        }

        public async Task<CheckEnoughStockRes> CheckEnoughStockAsync(CheckEnoughStockReq command)
        {
            var request = new CheckEnoughStockRequest
            {
                StoreId = command.StoreId.ToString()
            };
            var checkItems = command.CheckItems.Select(x => new InventoryService.Grpc.Protos.Inventory.Internal.AmountVariant
            {
                VariantId = x.VariantId.ToString(), // Giả sử trong proto bạn để string
                Amount = x.Amount
            });

            request.CheckItems.AddRange(checkItems);
            var respone = await _client.CheckEnoughStockAsync(request);
            return new CheckEnoughStockRes
            {
                IsEnough = respone.IsEnough,
                ShortageItems = respone.ShortageItems.Select(x => new Application.Dtos.Inventory.AmountVariant
                {
                    VariantId = Guid.Parse(x.VariantId),
                    Amount = x.Amount
                }).ToList()
            };
        }

        public async Task DecreaseStockByOrder(DecreaseStockByOrderReq command)
        {
            // 1. Khởi tạo object trước
            var request = new DecreaseStockByOrderRequest
            {
                StoreId = command.StoreId.ToString()
            };

            // 2. Sử dụng AddRange để thêm danh sách items vào trường repeated
            var decreaseItems = command.Items.Select(x => new InventoryService.Grpc.Protos.Inventory.Internal.AmountVariant
            {
                VariantId = x.VariantId.ToString(), // Giả sử trong proto bạn để string
                Amount = x.Amount
            });

            request.DecreaseItems.AddRange(decreaseItems);
            await _client.DecreaseStockByOrderAsync(request);
        }

        public async Task<CheckEnoughStockRes> ReserveStockAsync(ReserveStockReq command)
        {
            var request = new ReserveStockRequest
            {
                StoreId = command.StoreId.ToString(),
                OrderId = command.OrderId.ToString()
            };

            var items = command.Items.Select(x => new InventoryService.Grpc.Protos.Inventory.Internal.AmountVariant
            {
                VariantId = x.VariantId.ToString(),
                Sku = x.Sku,
                Amount = x.Amount
            });

            request.Items.AddRange(items);

            var response = await _client.ReserveStockAsync(request);

            return new CheckEnoughStockRes
            {
                IsEnough = response.IsEnough,
                ShortageItems = response.ShortageItems.Select(x => new Application.Dtos.Inventory.AmountVariant
                {
                    VariantId = Guid.Parse(x.VariantId),
                    Amount = x.Amount
                }).ToList()
            };
        }

        public async Task<ReleaseStockRes> ReleaseStockAsync(ReleaseStockReq command)
        {
            var request = new ReleaseStockRequest
            {
                StoreId = command.StoreId.ToString(),
                OrderId = command.OrderId.ToString()
            };

            var response = await _client.ReleaseStockAsync(request);

            return new ReleaseStockRes
            {
                Success = response.Success,
                Message = response.Message
            };
        }
    }
}