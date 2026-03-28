namespace SupplierService.Application.ClientInterfaces;

public interface IFranchiseServiceClient
{
    Task<bool> GetFranchiseByIdAsync(Guid franchiseId);
}