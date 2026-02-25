using InventoryService.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Application.ClientInterface
{
    public interface IIdentityServiceClient
    {
        Task<UserRep> GetManager(GetManagerReq request);
    }
}
