using EngagementService.Domain.Entities;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IVoucherItemRepository : IGenericRepository<Domain.Entities.VoucherItem>
    {
        Task<IEnumerable<VoucherItem>> GetByVoucherId(Guid voucherId);
    }
}
