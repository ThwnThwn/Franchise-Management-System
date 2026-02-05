using InventoryService.Application.Dtos.Product;
using InventoryService.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.ClientInterface
{
    public interface IProductServiceClient
    {
        Task<ProductVariantRespone> GetVariantById(Guid request);
    }
}
