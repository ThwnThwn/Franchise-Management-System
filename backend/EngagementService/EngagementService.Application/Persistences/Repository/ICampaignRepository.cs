using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        Task<Campaign?> GetCampaignByName(string name);
        Task<Campaign?> GetCampaignById(Guid id);
        Task<IEnumerable<Campaign>> GetAllCampaign();
    }
}
