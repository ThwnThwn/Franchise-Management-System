using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Abstraction
{
    public interface IMembershipTierSyncService
    {
        Task SyncAllAsync();
    }
}
