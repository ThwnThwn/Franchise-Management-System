using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Entities;

namespace FranchiseService.Application.Repositories
{
    public interface IStoreRepository : IGenericRepository<Store>
    {
        // Lấy store kèm theo giờ hoạt động
        Task<Store?> GetStoreWithHoursAsync(Guid storeId, CancellationToken cancellationToken);

        // Tìm store gần nhất
        Task<List<Store>> GetNearbyStoresAsync(double lat, double userLong, double radiusKm, int limit);

        // Lấy danh sách phân trang
        Task<PagedResult<Store>> GetPagedStoresAsync(string keyword, int pageIndex, int pageSize, CancellationToken cancellationToken);

        // Lấy chi tiết (đã có GetStoreWithHoursAsync ở bài trước, ta dùng lại hoặc viết mới nếu cần logic khác)
        Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}