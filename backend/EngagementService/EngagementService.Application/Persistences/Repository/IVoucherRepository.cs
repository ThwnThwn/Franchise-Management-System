<<<<<<< HEAD
﻿using EngagementService.Domain.Entities;
=======
﻿using EngagementService.Application.Dtos.Voucher;
using EngagementService.Domain.Entities;
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
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
<<<<<<< HEAD
=======

        Task UpdateVoucherAsync(UpdateVoucherRequest voucherUpdate);
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
    }
}
