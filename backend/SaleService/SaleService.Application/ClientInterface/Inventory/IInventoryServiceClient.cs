using SalesService.Application.Dtos.Inventory;

namespace SalesService.Application.ClientInterface.Inventory
{
    public interface IInventoryServiceClient
    {
        Task DecreaseStockByOrder(DecreaseStockByOrderReq request);
        Task<CheckEnoughStockRes> CheckEnoughStockAsync(CheckEnoughStockReq request);
        Task<CheckEnoughStockRes> ReserveStockAsync(ReserveStockReq request);
        Task<ReleaseStockRes> ReleaseStockAsync(ReleaseStockReq request);
    }

    public class ReserveStockReq
    {
        public Guid StoreId { get; set; }
        public Guid OrderId { get; set; }
        public List<AmountVariant> Items { get; set; } = new();
    }

    public class ReserveStockResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class ReleaseStockReq
    {
        public Guid StoreId { get; set; }
        public Guid OrderId { get; set; }
    }

    public class ReleaseStockRes
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
