using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IMembershipTierRepository : IGenericRepository<Domain.Entities.MembershipTier>
    {
        Task<MembershipTier?> GetDefaultTierAsync();
    }
}
