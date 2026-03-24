namespace WarehouseService.Application.ClientInterfaces
{
    public interface IFranchiseServiceClient
    {
        Task<bool> StoreExistsAsync(Guid storeId);
    }
}
