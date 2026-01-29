using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Core.Pagination;

namespace BuildingBlocks.Core.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> source,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // 1. Đếm tổng số record trong DB (Query Count)
            var count = await source.CountAsync(cancellationToken);

            // 2. Nếu không có dữ liệu thì return luôn cho lẹ, đỡ tốn query data
            if (count == 0)
            {
                return new PagedResult<T>(new List<T>(), 0, pageIndex, pageSize);
            }

            // 3. Tính toán Skip/Take để lấy dữ liệu trang hiện tại (Query Data)
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // 4. Đóng gói vào PagedResult
            return new PagedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}