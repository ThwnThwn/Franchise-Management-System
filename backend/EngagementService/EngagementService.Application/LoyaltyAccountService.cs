using EngagementService.Application.Abstraction;
using EngagementService.Application.Persistences.Cache;
using EngagementService.Application.Persistences.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application
{
    public class LoyaltyAccountService : ILoyaltyAccountService
    {
        private readonly ILoyaltyAccountRepository _loyaltyAccountRepository;
        private readonly IPointRuleCache _pointRuleCache;

        public LoyaltyAccountService(ILoyaltyAccountRepository loyaltyAccountRepository, IPointRuleCache pointRuleCache)
        {
            _loyaltyAccountRepository = loyaltyAccountRepository;
            _pointRuleCache = pointRuleCache;
        }
    }
}
