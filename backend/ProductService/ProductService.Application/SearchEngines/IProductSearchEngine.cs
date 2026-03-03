using BuildingBlocks.Core.Pagination;
using ProductService.Application.Dtos;
using ProductService.Domain.Entities;

namespace ProductService.Application.SearchEngines
{
    public interface IProductSearchEngine
    {
        /// <summary>
        /// Tìm kiếm Full-text và phân trang.
        /// Thay thế logic "ToListAsync rồi mới filter" cũ.
        /// </summary>
        Task<PagedResult<ProductDto>> GetAllProductWithPagingByAsync(
            string? keyword,
            Guid? categoryId,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy trực tiếp từ Elastic bằng ID
        /// </summary>
        Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}