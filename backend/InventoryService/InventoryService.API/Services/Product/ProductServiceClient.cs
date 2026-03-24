using Grpc.Net.Client;
using InventoryService.Application.ClientInterface;
using InventoryService.Application.Constants;
using InventoryService.Application.Dtos.Product;
using MediatR;
using ProductService.Grpc;

namespace InventoryService.API.Services.Product
{
    public class ProductServiceClient(ProductVariantGrpc.ProductVariantGrpcClient client) : IProductServiceClient
    {
        public async Task<ProductVariantRespone> GetVariantById(Guid request)
        {
            var req = new GetProductVariantByIdRequest
            {
                ProductVariantId = request.ToString()
            };
            var respone = await client.GetByIdAsync(req);
            return new ProductVariantRespone()
            {
                Id = Guid.Parse(respone.Id),
                Sku = respone.Sku,
                Name = respone.Name,
                Attributes = respone.Attributes,
                BasePrice = (decimal)respone.BasePrice,
                Cost = (decimal)respone.Cost,
                IsActive = respone.IsActive,
                ProductId = Guid.Parse(respone.ProductId)
            };  
        }

        public async Task UpdateVariantCostsAsync(Dictionary<Guid, decimal> variantCosts, CancellationToken cancellationToken = default)
        {
            var request = new UpdateVariantCostsRequest();
            foreach (var (variantId, newCost) in variantCosts)
            {
                request.Updates.Add(new VariantCostUpdate
                {
                    VariantId = variantId.ToString(),
                    NewCost = (double)newCost
                });
            }
            await client.UpdateVariantCostsAsync(request, cancellationToken: cancellationToken);
        }
    }
}
