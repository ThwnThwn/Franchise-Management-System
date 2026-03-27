using EngagementService.Application.Constants;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus
{
    public class UseVoucherHandler : IRequestHandler<UseVoucherCommand, Unit>
    {
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly IEngagementUow _uow;

        public UseVoucherHandler(IUserVoucherRepository userVoucherRepository, IVoucherUsageRepository voucherUsageRepository, IEngagementUow uow)
        {
            _userVoucherRepository = userVoucherRepository;
            _voucherUsageRepository = voucherUsageRepository;
            _uow = uow;
        }

        public async Task<Unit> Handle(UseVoucherCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var userVouchers = await _userVoucherRepository.GetByVoucherIdsAsync(request.VoucherIds, request.UserId);

            if (userVouchers == null || userVouchers.Count == 0)
                throw new KeyNotFoundException(VoucherMsg.NotFound);

            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                List<VoucherUsage> voucherUsages = new List<VoucherUsage>();
                foreach (var userVoucher in userVouchers)
                {
                    var voucherUsage = new VoucherUsage
                    {
                        UserVoucherId = userVoucher.Id,
                        VoucherId = userVoucher.VoucherId,
                        AppliedAt = DateTimeOffset.UtcNow,
                        OrderId = request.OrderId,
                    };

                    voucherUsages.Add(voucherUsage);
                    userVoucher.Status = UserVoucherStatus.USED;

                }
                _userVoucherRepository.UpdateRangeAsync(userVouchers, cancellationToken);
                await _voucherUsageRepository.AddRangeAsync(voucherUsages, cancellationToken);

                await _uow.SaveChangesAsync(cancellationToken);
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
