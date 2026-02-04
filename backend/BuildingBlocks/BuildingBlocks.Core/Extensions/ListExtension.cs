
using BuildingBlocks.Core.Pagination;

namespace BuildingBlocks.Core.Extensions
{
    public static class ListExtension
    {
        public static PagedResult<T> ToPagedList<T>(
            this IEnumerable<T> source,
            int pageIndex,
            int pageSize)
        {
            var list = source as IList<T> ?? source.ToList();
            var count = list.Count;

            if (count == 0)
            {
                return new PagedResult<T>(new List<T>(), 0, pageIndex, pageSize);
            }

            var items = list
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>(items, count, pageIndex, pageSize);
        }
    }
}