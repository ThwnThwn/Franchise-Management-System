using EngagementService.Infrastructure.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace EngagementService.API.Services.Voucher
{
    public class VoucherGrpcService : VoucherGrpc.VoucherGrpcBase
    {
        private readonly IMediator _mediator;
        public VoucherGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async override Task<Empty> CreateVoucher(CreateVoucherRequest request, ServerCallContext context)
        {

            var createReq = new Application.Dtos.Voucher.CreateVoucherRequest()
            {
                Code = request.Code,
                Category = (Domain.Constants.VoucherCategory)request.Category,
                DiscountValue = string.IsNullOrEmpty(request.DiscountValue) ? null : decimal.Parse(request.DiscountValue),
                MaxDiscountValue = string.IsNullOrEmpty(request.MaxDiscountValue) ? null : decimal.Parse(request.MaxDiscountValue),
                MinOrderValue = string.IsNullOrEmpty(request.MinOrderValue) ? null : decimal.Parse(request.MinOrderValue),
                PointCost = string.IsNullOrEmpty(request.PointCost) ? null : decimal.Parse(request.PointCost),
                InitQuantity = request.InitQuantity == 0 ? null : request.InitQuantity,
                MaxUsagePerUser = request.MaxUsagePerUser == 0 ? null : request.MaxUsagePerUser,
                EndDate = request.EndDate != null ? request.EndDate.ToDateTimeOffset() : null,
                Type = (Domain.Constants.VoucherDiscountType)request.Type,
                VoucherItems = request.VoucherItems.Select(item => new Application.Dtos.Voucher.CreateVoucherItemReq()
                {
                    VariantId = Guid.Parse(item.VariantId),
                    Quantity = item.Quantity,
                    Type = (Domain.Constants.VoucherItemType)item.Type
                }).ToList()
            };

            await _mediator.Send(new Application.Features.Voucher.Commands.Create.CreateVoucherCommand(Request: createReq));
            return new Empty();
        }
    }
}
