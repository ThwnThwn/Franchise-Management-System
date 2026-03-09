using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Queries.GetMyVoucher
{
    public class GetMyVoucherHandler : IRequestHandler<GetMyVoucherQuery, List<Domain.Entities.Voucher>>
    {

        private readonly IUserVoucherRepository _userVoucherRepository;

        public GetMyVoucherHandler(IUserVoucherRepository userVoucherRepository)
        {
            _userVoucherRepository = userVoucherRepository;
        }
        public async Task<List<Domain.Entities.Voucher>> Handle(GetMyVoucherQuery request, CancellationToken cancellationToken)
        {
            var userVouchers = await _userVoucherRepository.GetByUserIdAsync(request.userID);

            return userVouchers.Select(uv => uv.Voucher).ToList();
        }
    }
}
