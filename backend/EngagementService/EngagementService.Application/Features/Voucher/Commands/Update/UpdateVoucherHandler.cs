using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Update
{
    public class UpdateVoucherHandler : IRequestHandler<UpdateVoucherCommand, Unit>
    {
        private readonly IVoucherRepository _voucherRepository;
        public UpdateVoucherHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<Unit> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            var updateRequest = request.Request;
            if (updateRequest.EndDate.HasValue && updateRequest.EndDate.Value <= DateTimeOffset.UtcNow)
                throw new ArgumentException(VoucherMsg.InvalidEndDate);
            await _voucherRepository.UpdateVoucherAsync(updateRequest);

            return Unit.Value;
        }
    }
}
