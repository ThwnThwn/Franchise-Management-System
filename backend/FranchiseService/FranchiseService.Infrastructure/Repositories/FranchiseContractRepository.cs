using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories;

public class FranchiseContractRepository : GenericRepository<FranchiseContract>, IFranchiseContractRepository
{
    public FranchiseContractRepository(IFranchiseDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FranchiseContract?> GetByDocusealSubmissionAsync(int submissionId, CancellationToken cancellationToken)
    {
        return await _dbContext.FranchiseContracts
            .FirstOrDefaultAsync(c => c.DocusealSubmissionId == submissionId, cancellationToken);
    }

    public async Task<PagedResult<FranchiseContract>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        ContractStatus? status,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.FranchiseContracts
            .Include(c => c.Application)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        query = query.OrderByDescending(c => c.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<FranchiseContract?> GetWithApplicationAsync(Guid contractId, CancellationToken cancellationToken)
    {
        return await _dbContext.FranchiseContracts
            .Include(c => c.Application)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);
    }
}
