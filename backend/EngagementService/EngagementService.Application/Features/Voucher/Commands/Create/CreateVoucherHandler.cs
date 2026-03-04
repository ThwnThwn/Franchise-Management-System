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
<<<<<<< HEAD
                    throw new ArgumentException(VoucherCodeMsg.CodeExisted);
=======
                    throw new ArgumentException(VoucherMsg.CodeExisted);
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
                }
            }
            else code = GenerateVoucherCode();

            Domain.Entities.Voucher newVoucher;
<<<<<<< HEAD
            switch (request.Category)
            {
                case VoucherCategory.SHIPPING:
                case VoucherCategory.DISCOUNT:
=======
            switch (request.Type)
            {
                case VoucherType.SHIPPING_FIXED_AMOUNT:
                case VoucherType.SHIPPING_PERCENTAGE:
                case VoucherType.DISCOUNT_PERCENTAGE:
                case VoucherType.DISCOUNT_FIXED_AMOUNT:

                    if(request.DiscountValue == null || request.DiscountValue <= 0)
                        throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

                    if ((request.Type == VoucherType.DISCOUNT_PERCENTAGE || request.Type == VoucherType.SHIPPING_PERCENTAGE) && (request.DiscountValue <= 0 || request.DiscountValue > 100))
                        throw new ArgumentException(VoucherMsg.InvalidDiscountValue);

                    if (request.EndDate < DateTimeOffset.UtcNow)
                        throw new ArgumentException(VoucherMsg.InvalidEndDate);
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960

                    newVoucher = await CreateSimpleVoucher(request, code);
                    await _voucherRepository.AddAsync(newVoucher);
                    break;
<<<<<<< HEAD
                case VoucherCategory.GIFT:

=======

                case VoucherType.GIFT:

                    if (request.VoucherItems == null || !request.VoucherItems.Any() || !request.VoucherItems.Any(t => t.Type == VoucherItemType.ITEM_REQUIRED) || !request.VoucherItems.Any(t => t.Type == VoucherItemType.ITEM_GIFT))
                        throw new ArgumentException(VoucherMsg.EmptyVoucherItems);

                    if (!request.VoucherItems.All(item => item.Quantity > 0)) 
                        throw new ArgumentException(VoucherMsg.InvalidVoucherItemQuantity);
                    
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
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
<<<<<<< HEAD
                    throw new ArgumentException("Invalid voucher category");
=======
                    throw new ArgumentException(VoucherMsg.InvalidType);
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
            }
            return Unit.Value;
        }

<<<<<<< HEAD
        //private async Task<Domain.Entities.Voucher> CreateGiftVoucher(CreateVoucherRequest request, string code)
        //{
        //    var voucherId = Guid.NewGuid();
        //    var voucherItems = request.VoucherItems.Select(item => new VoucherItem()
        //    {
        //        Id = Guid.NewGuid(),
        //        VariantId = item.VariantId,
        //        Quantity = item.Quantity,
        //        Type = item.Type,
        //    }).ToList();
        //    var newVoucher = new Domain.Entities.Voucher()
        //    {
        //        Id = voucherId,
        //        Code = code,
        //        Category = request.Category,
        //        Type = request.Type,
        //        DiscountValue = request.DiscountValue,
        //        MaxDiscountValue = request.MaxDiscountValue,
        //        PointCost = request.PointCost,
        //        MinOrderValue = request.MinOrderValue,
        //        InitQuantity = request.InitQuantity,
        //        CurrentQuantity = request.InitQuantity,
        //        MaxUsagePerUser = request.MaxUsagePerUser,
        //        EndDate = request.EndDate,
        //        VoucherItems = voucherItems
        //    };

        //}

        private async Task<Domain.Entities.Voucher> CreateSimpleVoucher(CreateVoucherRequest request, string code)
        {
=======

        private async Task<Domain.Entities.Voucher> CreateSimpleVoucher(CreateVoucherRequest request, string code)
        {
            if((request.Type == VoucherType.DISCOUNT_PERCENTAGE || request.Type == VoucherType.SHIPPING_PERCENTAGE) && (!request.DiscountValue.HasValue || request.DiscountValue <= 0 || request.DiscountValue > 100))
                throw new ArgumentException(VoucherMsg.InvalidDiscountValue);
            
            if(request.EndDate < DateTimeOffset.UtcNow)
                throw new ArgumentException(VoucherMsg.InvalidEndDate);

>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
            return new Domain.Entities.Voucher()
            {
                Id = Guid.NewGuid(),
                Code = code,
<<<<<<< HEAD
                Category = request.Category,
=======
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
                Type = request.Type,
                DiscountValue = request.DiscountValue,
                MaxDiscountValue = request.MaxDiscountValue,
                PointCost = request.PointCost,
                MinOrderValue = request.MinOrderValue,
                InitQuantity = request.InitQuantity,
                CurrentQuantity = request.InitQuantity,
                MaxUsagePerUser = request.MaxUsagePerUser,
                EndDate = request.EndDate
<<<<<<< HEAD

            };
            //await _voucherRepository.AddAsync(newVoucher);
=======
            };
>>>>>>> 67995ea2d691e474b75c2df4113ec3b4637af960
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
