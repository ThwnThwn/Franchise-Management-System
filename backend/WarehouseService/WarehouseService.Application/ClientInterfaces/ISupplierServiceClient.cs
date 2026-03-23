namespace WarehouseService.Application.ClientInterfaces
{
    public interface ISupplierServiceClient
    {
        public Task<bool> SupplierExistsAsync(Guid supplierId);
    }
}
