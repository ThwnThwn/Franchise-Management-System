using SalesService.Application.Dtos.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesService.Application.ClientInterface.Inventory
{
    public interface IInventoryServiceClient
    {
        Task DecreaseStockByOrder(DecreaseStockByOrderReq request);
        Task<CheckEnoughStockRes> CheckEnoughStockAsync(CheckEnoughStockReq request);
        Task<ReserveStockResponseDto> ReserveStockAsync(ReserveStockReq request);
    }
    
    public class ReserveStockReq {
        public Guid StoreId { get; set; }
        public Guid OrderId { get; set; }
        public List<AmountVariant> Items { get; set; } = new();
    }
    
    public class ReserveStockResponseDto {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    
}
