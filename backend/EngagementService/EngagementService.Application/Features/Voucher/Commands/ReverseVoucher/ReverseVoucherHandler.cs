using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngagementService.Application.Features.Voucher.Commands.ReverseVoucher
{
    public class ReverseVoucherHandler : IRequestHandler<ReverseVoucherCommand, Unit>
    {
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IEngagementUow _uow;

        public ReverseVoucherHandler(IVoucherUsageRepository voucherUsageRepository, IUserVoucherRepository userVoucherRepository, IEngagementUow uow)
        {
            _voucherUsageRepository = voucherUsageRepository;
            _userVoucherRepository = userVoucherRepository;
            _uow = uow;
        }

        public async Task<Unit> Handle(ReverseVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucherUsages = await _voucherUsageRepository.GetByOrderIdAsync(request.OrderId);

            if (voucherUsages == null || !voucherUsages.Any())
            {
                throw new DirectoryNotFoundException(VoucherMsg.NotFound);
            }

            await _uow.BeginTransactionAsync(cancellationToken);

            try
            {
                List<UserVoucher> userVouchers = new List<UserVoucher>();
                foreach (var VoucherUsage in voucherUsages)
                {
                    var uv = await _userVoucherRepository.GetByIdAsync(VoucherUsage.UserVoucherId);
                    if (uv != null)
                    {
                        uv.Status = Domain.Constants.UserVoucherStatus.AVAILABLE;
                        userVouchers.Add(uv);
                    }
                }
                _userVoucherRepository.UpdateRangeAsync(userVouchers, cancellationToken);
                _voucherUsageRepository.DeleteRangeAsync(voucherUsages, cancellationToken);
                await _uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync(cancellationToken);
                throw;
            }

            return Unit.Value;
        }
    }
}
