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
                    throw new ArgumentException(VoucherCodeMsg.CodeExisted);
                }
            }
            else code = GenerateVoucherCode();

            Domain.Entities.Voucher newVoucher;
            switch (request.Category)
            {
                case VoucherCategory.SHIPPING:
                case VoucherCategory.DISCOUNT:

                    newVoucher = await CreateSimpleVoucher(request, code);
                    await _voucherRepository.AddAsync(newVoucher);
                    break;
                case VoucherCategory.GIFT:

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
                    throw new ArgumentException("Invalid voucher category");
            }
            return Unit.Value;
        }

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
            return new Domain.Entities.Voucher()
            {
                Id = Guid.NewGuid(),
                Code = code,
                Category = request.Category,
                Type = request.Type,
                DiscountValue = request.DiscountValue,
                MaxDiscountValue = request.MaxDiscountValue,
                PointCost = request.PointCost,
                MinOrderValue = request.MinOrderValue,
                InitQuantity = request.InitQuantity,
                CurrentQuantity = request.InitQuantity,
                MaxUsagePerUser = request.MaxUsagePerUser,
                EndDate = request.EndDate

            };
            //await _voucherRepository.AddAsync(newVoucher);
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
