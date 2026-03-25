using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;

namespace FranchiseService.Application.Repositories;

public interface IFranchiseApplicationRepository : IGenericRepository<FranchiseApplication>
{
    /// <summary>
    /// Lấy danh sách hồ sơ phân trang, có thể lọc theo status.
    /// </summary>
    Task<PagedResult<FranchiseApplication>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        FranchiseApplicationStatus? status,
        string? keyword,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy chi tiết hồ sơ kèm danh sách hợp đồng liên quan.
    /// </summary>
    Task<FranchiseApplication?> GetWithContractsAsync(Guid id, CancellationToken cancellationToken);
}
