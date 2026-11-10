using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Pagination;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Enums;
using FranchiseService.Domain.Models;
using FranchiseService.Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories;

public class FranchiseApplicationRepository : GenericRepository<FranchiseApplication>, IFranchiseApplicationRepository
{
    public FranchiseApplicationRepository(IFranchiseDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedResult<FranchiseApplication>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        FranchiseApplicationStatus? status,
        string? keyword,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.FranchiseApplications
            .Include(a => a.Contracts)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lower = keyword.ToLower();
            query = query.Where(a =>
                a.FullName.ToLower().Contains(lower) ||
                a.Email.ToLower().Contains(lower) ||
                a.Phone.Contains(lower) ||
                a.IdentityNumber.Contains(lower));
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        return await query.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
    }

    public async Task<FranchiseApplication?> GetWithContractsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.FranchiseApplications
            .Include(a => a.Contracts)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
