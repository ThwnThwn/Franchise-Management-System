using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupplierService.Application.ClientInterfaces
{
    public interface IFranchiseServiceClient
    {
        Task<bool> GetFranchiseByIdAsync(Guid franchiseId);
    }
}
