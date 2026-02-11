using System.Text.Json.Serialization;

namespace BuildingBlocks.Core.Pagination
{
    /// <summary>
    /// Kết quả phân trang chuẩn.
    /// Property "Page" serialize thành "pageNumber" để khớp FE.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Trang hiện tại (1-indexed).
        /// Serialize thành "pageNumber" để khớp với FE PageResult.
        /// </summary>
        [JsonPropertyName("pageNumber")]
        public int Page { get; }

        public int PageSize { get; }
        public long TotalCount { get; }
        public int TotalPages { get; }

        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public PagedResult(IReadOnlyList<T> items, long totalCount, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = pageIndex;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public static PagedResult<T> Create(IReadOnlyList<T> items, long totalCount, int pageIndex, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageIndex, pageSize);
        }
    }
}