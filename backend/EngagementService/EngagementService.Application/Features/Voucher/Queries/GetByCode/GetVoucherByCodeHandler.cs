using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Queries.GetByCode
{
    public class GetVoucherByCodeHandler : IRequestHandler<GetVoucherByCodeQuery, Domain.Entities.Voucher>
    {
        private readonly IVoucherRepository _voucherRepository;
        public GetVoucherByCodeHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<Domain.Entities.Voucher> Handle(GetVoucherByCodeQuery request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetVoucherByCodeAsync(request.code);
            if (voucher == null || voucher.IsActive == false) throw new DirectoryNotFoundException(VoucherMsg.NotFound);
            return voucher;
        }
    }
}
