using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.ExternalServices
{
    public interface IEngagementServiceClient
    {
        Task<string> CreateLoyaltyAccount(Guid userId);
    }
}
