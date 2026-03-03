using IdentityService.Application.Dtos.Order.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.ExternalServices
{
    public interface ISaleServiceClient
    {
        Task<IEnumerable<UserCountOrderRes>> GetCountOrderByUserIds(List<Guid> UserIds);
    }
}
