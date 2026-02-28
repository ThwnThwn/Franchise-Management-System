using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using EngagementService.Infrastucture.AppDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Infrastucture.Persistences.Repository
{
    public class PointLogRepository : GenericRepository<PointLog>, IPointLogRepository
    {
        public PointLogRepository(IEngagementDbContext dbContext) : base(dbContext)
        {
        }
    }
}
