using EngagementService.Application.Dtos.Voucher;
using EngagementService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Persistences.Repository
{
    public interface IVoucherRepository : IGenericRepository<Domain.Entities.Voucher>
    {
        Task<Voucher?> GetVoucherByCodeAsync(string code);

        Task UpdateVoucherAsync(UpdateVoucherRequest voucherUpdate);

        Task<Voucher?> GetForUpdateAsync(Guid voucherId, CancellationToken cancellationToken);
    }
}
