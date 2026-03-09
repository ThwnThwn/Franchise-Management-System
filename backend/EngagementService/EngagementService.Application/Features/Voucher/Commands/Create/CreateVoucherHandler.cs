using EngagementService.Application.Constants;
using EngagementService.Application.Dtos.Voucher;
using EngagementService.Application.Persistences.Repository;
using EngagementService.Domain.Constants;
using EngagementService.Domain.Entities;
using MediatR;

namespace EngagementService.Application.Features.Voucher.Commands.Create
{
    public class CreateVoucherHandler : IRequestHandler<CreateVoucherCommand, Unit>
    {
        private readonly IVoucherItemRepository _voucherItemRepository;
        private readonly IVoucherRepository _voucherRepository;
        public CreateVoucherHandler(IVoucherItemRepository voucherItemRepository, IVoucherRepository voucherRepository)
        {
            _voucherItemRepository = voucherItemRepository;
            _voucherRepository = voucherRepository;
        }
        public async Task<Unit> Handle(CreateVoucherCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            //kiểm tra coi có truyền code hay không, có thì kiểm tra đã tồn tại chưa, chưa thì tự generate
            var code = request.Code;

            if (!string.IsNullOrWhiteSpace(code))
            {
                var existingVoucher = await _voucherRepository.GetVoucherByCodeAsync(code);
                if (existingVoucher != null)
                {
                    throw new ArgumentException(VoucherMsg.CodeExisted);
                }
            }
            else code = GenerateVoucherCode();

            if (request.InitQuantity < 0)
                throw new ArgumentException(VoucherMsg.InvalidInitQuantity);

            if (request.MaxUsagePerUser <= 0)
                throw new ArgumentException(VoucherMsg.InvalidMaxUsagePerUser);

            Domain.Entities.Voucher newVoucher;
            switch (request.Type)
            {
                case VoucherType.SHIPPING_FIXED_AMOUNT:
                case VoucherType.SHIPPING_PERCENTAGE:
                case VoucherType.DISCOUNT_PERCENTAGE:
                case VoucherType.DISCOUNT_FIXED_AMOUNT:

                    if (request.DiscountValue == null || request.DiscountValue <= 0)
                        throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

                    if ((request.Type == VoucherType.DISCOUNT_PERCENTAGE || request.Type == VoucherType.SHIPPING_PERCENTAGE) && (request.DiscountValue <= 0 || request.DiscountValue > 100))
                        throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

                    newVoucher = await CreateSimpleVoucher(request, code);
                    await _voucherRepository.AddAsync(newVoucher);
                    break;

                case VoucherType.GIFT:

                    if (request.VoucherItems == null || !request.VoucherItems.Any() || !request.VoucherItems.Any(t => t.Type == VoucherItemType.ITEM_GIFT))
                        throw new ArgumentException(VoucherMsg.EmptyVoucherItems);

                    if (!request.VoucherItems.All(item => item.Quantity > 0))
                        throw new ArgumentException(VoucherMsg.InvalidVoucherItemQuantity);

                    //tạo voucher mới
                    newVoucher = await CreateSimpleVoucher(request, code);

                    //tạo voucher item
                    var voucherItems = request.VoucherItems.Select(item => new VoucherItem()
                    {
                        
                        Id = Guid.NewGuid(),
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Type = item.Type,
                        VoucherId = newVoucher.Id
                    }).ToList();
                    newVoucher.VoucherItems = voucherItems;

                    await _voucherRepository.AddAsync(newVoucher);
                    break;

                default:
                    throw new ArgumentException(VoucherMsg.InvalidType);
            }
            return Unit.Value;
        }


        private async Task<Domain.Entities.Voucher> CreateSimpleVoucher(CreateVoucherRequest request, string code)
        {
            if ((request.Type == VoucherType.DISCOUNT_PERCENTAGE || request.Type == VoucherType.SHIPPING_PERCENTAGE) && (!request.DiscountValue.HasValue || request.DiscountValue <= 0 || request.DiscountValue > 100))
                throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

            if (request.EndDate < DateTimeOffset.UtcNow)
                throw new ArgumentException(VoucherMsg.InvalidEndDate);

            var isGiftVoucher = request.Type == VoucherType.GIFT;

            return new Domain.Entities.Voucher()
            {
                Id = Guid.NewGuid(),
                Code = code,
                Type = request.Type,
                DiscountValue = isGiftVoucher ? null : request.DiscountValue,
                MaxDiscountValue = isGiftVoucher ? null : request.MaxDiscountValue,
                PointCost = request.PointCost,
                MinOrderValue = request.MinOrderValue,
                InitQuantity = request.InitQuantity,
                CurrentQuantity = request.InitQuantity,
                MaxUsagePerUser = request.MaxUsagePerUser,
                EndDate = request.EndDate
            };
        }

        private string GenerateVoucherCode(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
