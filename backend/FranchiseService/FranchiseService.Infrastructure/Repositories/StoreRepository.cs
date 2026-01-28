using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        public StoreRepository(IFranchiseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Store?> GetStoreWithHoursAsync(Guid storeId, CancellationToken cancellationToken)
        {
            return await _dbContext.Stores
                .Include(s => s.StoreHours) // Load luôn giờ để check
                .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);
        }

        public async Task<List<Store>> GetNearbyStoresAsync(double userLat, double userLong, double radiusKm, int limit)
        {
            // 1. Định nghĩa hằng số cho công thức
            // 6371 là bán kính trái đất (km)
            const double earthRadiusKm = 6371;
            var openStatus = StoreStatus.Open;

            // 2. Viết LINQ
            var query = _dbContext.Stores
                .AsNoTracking()
                .Where(s => s.Status == (int)openStatus)
                .Select(s => new
                {
                    Store = s,
                    // Công thức Haversine tính ngay trong LINQ
                    Distance = earthRadiusKm * Math.Acos(
                        Math.Cos(userLat * Math.PI / 180) * Math.Cos((double)(s.Latitude ?? 0) * Math.PI / 180)
                            * Math.Cos(((double)(s.Longitude ?? 0) * Math.PI / 180) - (userLong * Math.PI / 180))
                            + Math.Sin(userLat * Math.PI / 180) * Math.Sin((double)(s.Latitude ?? 0) * Math.PI / 180)
                    )
                })
                // 3. Filter theo bán kính
                .Where(x => x.Distance <= radiusKm)
                // 4. Sắp xếp gần nhất trước
                .OrderBy(x => x.Distance)
                .Take(limit);

            // 5. Execute và lấy dữ liệu
            var result = await query.ToListAsync();

            // 6. Trả về List<Store> (bỏ cái wrapper distance đi)
            return result.Select(x => x.Store).ToList();
        }

        public async Task<PagedResult<Store>> GetPagedStoresAsync(string? keyword, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            // Query cơ bản
            var query = _dbContext.Stores
                .AsNoTracking()
                .Where(s => s.Status == (int)StoreStatus.Open);

            // Filter Keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(lowerKeyword)
                                      || s.Address.ToLower().Contains(lowerKeyword));
            }

            // Sort (Mới nhất lên đầu)
            query = query.OrderByDescending(s => s.CreatedAt);

            return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        // IMPLEMENT 2: GetByIdAsync
        public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Stores
                .Include(s => s.StoreHours)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
    }
}