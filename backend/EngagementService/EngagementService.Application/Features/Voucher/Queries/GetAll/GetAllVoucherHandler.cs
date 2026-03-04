using EngagementService.Application.Persistences.Repository;
using MediatR;
using System.Linq.Expressions;

namespace EngagementService.Application.Features.Voucher.Queries.GetAll
{
    public class GetAllVoucherHandler : IRequestHandler<GetAllVoucherQuery, IEnumerable<Domain.Entities.Voucher>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public GetAllVoucherHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<IEnumerable<Domain.Entities.Voucher>> Handle(GetAllVoucherQuery request, CancellationToken cancellationToken)
        {
            return await _voucherRepository.GetAllAsync(
                    includes: new Expression<Func<Domain.Entities.Voucher, object>>[]
                    {
                        x => x.VoucherItems
                    }
            );
        }
    }
}
