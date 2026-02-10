using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories
{
    public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
    {
        public ShiftRepository(IFranchiseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Shift?> GetPublishedShiftAsync(Guid userId, DateTime date, CancellationToken cancellationToken)
        {
            var targetDate = DateOnly.FromDateTime(date);

            return await _dbContext.Shifts
                .AsNoTracking()
                .Include(s => s.Store)
                .Include(s => s.ShiftAssignments)
                .Where(s => s.ShiftAssignments.Any(a => a.UserId == userId)
                         && s.ShiftDate == targetDate
                         && s.Status == (int)ShiftStatus.Published)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}