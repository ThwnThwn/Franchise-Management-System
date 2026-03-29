using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Queries.GetAll
{
    public class GetAllVoucherHandler : IRequestHandler<GetAllVoucherQuery, IEnumerable<Domain.Entities.Voucher>>
    {
        private readonly IVoucherRepository _voucherRepository;

        private readonly IVoucherQueueRepository _voucherQueueRepository;

        public GetAllVoucherHandler(IVoucherRepository voucherRepository, IVoucherQueueRepository voucherQueueRepository)
        {
            _voucherRepository = voucherRepository;
            _voucherQueueRepository = voucherQueueRepository;
        }
        public async Task<IEnumerable<Domain.Entities.Voucher>> Handle(GetAllVoucherQuery request, CancellationToken cancellationToken)
        {
            var vouchers = await _voucherRepository.GetAllVoucherAsync(cancellationToken);

            var voucherQueues = await _voucherQueueRepository.GetVouchersAndAmountInQueueAsync(cancellationToken);

            foreach (var voucher in vouchers)
            {
                var queueInfo = voucherQueues.FirstOrDefault(vq => vq.VoucherId == voucher.Id);
                if (queueInfo != null)
                {
                    voucher.CurrentQuantity -= queueInfo.Count;
                }
            }

            return vouchers;
        }
    }
}
