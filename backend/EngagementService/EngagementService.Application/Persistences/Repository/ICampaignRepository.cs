using System.Linq.Expressions;
using EngagementService.Domain.Entities;

namespace EngagementService.Application.Persistences.Repository
{
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        Task<Campaign?> GetCampaignByName(string name);

        Task<Campaign?> GetCampaignById(Guid id);

        Task<IEnumerable<Campaign>> GetAllCampaign(Expression<Func<Campaign, bool>>? predicate = null);
    }
}