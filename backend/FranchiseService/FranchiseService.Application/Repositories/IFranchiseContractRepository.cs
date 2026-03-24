using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Persistences.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;

namespace FranchiseService.Application.Repositories;

public interface IFranchiseContractRepository : IGenericRepository<FranchiseContract>
{
    /// <summary>
    /// Tìm contract theo DocuSeal Submission ID — dùng cho Webhook xử lý callback ký xong.
    /// </summary>
    Task<FranchiseContract?> GetByDocusealSubmissionAsync(int submissionId, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách hợp đồng phân trang, có thể lọc theo status.
    /// </summary>
    Task<PagedResult<FranchiseContract>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        ContractStatus? status,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy contract kèm Application để hiển thị thông tin đầy đủ.
    /// </summary>
    Task<FranchiseContract?> GetWithApplicationAsync(Guid contractId, CancellationToken cancellationToken);
}
