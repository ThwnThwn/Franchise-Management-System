using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FranchiseService.Application.Repositories;
using FranchiseService.Domain.Entities;
using FranchiseService.Domain.Enums;
using FranchiseService.Infrastructure.ApplicationDbContext;
using FranchiseService.Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FranchiseService.Infrastructure.Repositories
{
    public class ScheduleRepository : GenericRepository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(IFranchiseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Schedule?> GetPublishedShiftAsync(Guid userId, DateTime date, CancellationToken cancellationToken)
        {
            return await _dbContext.Schedules
                .Include(s => s.Store) // Load luôn thông tin Store để hiển thị
                .Where(s => s.UserId == userId
                         && s.ShiftDate == DateOnly.FromDateTime(date.Date)
                         && s.Status == (int)ScheduleStatus.Published)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}