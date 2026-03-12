using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Delete
{
    public class DeleteVoucherHandler : IRequestHandler<DeleteVoucherCommand, Unit>
    {
        private readonly IVoucherRepository _voucherRepository;

        public DeleteVoucherHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<Unit> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null) throw new DirectoryNotFoundException(VoucherMsg.NotFound);

            if (voucher.IsActive)
                throw new InvalidOperationException(VoucherMsg.ActiveVoucher);

            if (voucher.CurrentQuantity != voucher.InitQuantity)
                throw new ArgumentException(VoucherMsg.VoucherInUsed);

            await _voucherRepository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
