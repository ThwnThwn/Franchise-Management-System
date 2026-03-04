using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus
{
    public class UpdateVoucherStatusHandler : IRequestHandler<UpdateVoucherStatusCommand, Unit>
    {
        private readonly IVoucherRepository _voucherRepository;
        public UpdateVoucherStatusHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task<Unit> Handle(UpdateVoucherStatusCommand request, CancellationToken cancellationToken)
        {
            var updateRequest = request.Request;

            var voucher = await _voucherRepository.GetByIdAsync(updateRequest.Id);
            if(voucher == null)
                throw new ArgumentException(VoucherMsg.NotFound);

            voucher.IsActive = updateRequest.IsActive;

            await _voucherRepository.UpdateAsync(voucher);  

            return Unit.Value;
        }
    }
}
