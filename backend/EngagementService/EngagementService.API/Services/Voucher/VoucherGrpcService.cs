
using EngagementService.Application.Features.Voucher.Commands.Create;
using EngagementService.Application.Features.Voucher.Commands.Delete;
using EngagementService.Application.Features.Voucher.Commands.ReverseVoucher;
using EngagementService.Application.Features.Voucher.Commands.Update;
using EngagementService.Application.Features.Voucher.Commands.UpdateUserVoucherStatus;
using EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus;
using EngagementService.Application.Features.Voucher.Queries.GetAll;
using EngagementService.Application.Features.Voucher.Queries.GetByCode;
using EngagementService.Application.Features.Voucher.Queries.GetMyVoucher;
using EngagementService.Grpc.Protos.VoucherGrpc;
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
                Type = (Domain.Constants.VoucherType)request.Type,

                DiscountValue = string.IsNullOrEmpty(request.DiscountValue) ? null : decimal.Parse(request.DiscountValue),

                MaxDiscountValue = string.IsNullOrEmpty(request.MaxDiscountValue) ? null : decimal.Parse(request.MaxDiscountValue),

                MinOrderValue = string.IsNullOrEmpty(request.MinOrderValue) ? null : decimal.Parse(request.MinOrderValue),

                PointCost = string.IsNullOrEmpty(request.PointCost) ? null : decimal.Parse(request.PointCost),

                InitQuantity = request.InitQuantity == 0 ? null : request.InitQuantity,

                MaxUsagePerUser = request.MaxUsagePerUser == 0 ? null : request.MaxUsagePerUser,

                EndDate = request.EndDate != null ? request.EndDate.ToDateTimeOffset() : null,

                VoucherItems = request.VoucherItems.Select(item => new Application.Dtos.Voucher.CreateVoucherItemReq()
                {
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    Type = (Domain.Constants.VoucherItemType)item.Type
                }).ToList()
            };

            await _mediator.Send(new CreateVoucherCommand(Request: createReq));
            return new Empty();
        }

        public async override Task<Empty> UpdateVoucher(UpdateVoucherRequest request, ServerCallContext context)
        {
            var updateVoucher = new Application.Dtos.Voucher.UpdateVoucherRequest()
            {
                Id = Guid.Parse(request.Id),

                DiscountValue = string.IsNullOrEmpty(request.DiscountValue) ? null : decimal.Parse(request.DiscountValue),

                MaxDiscountValue = string.IsNullOrEmpty(request.MaxDiscountValue) ? null : decimal.Parse(request.MaxDiscountValue),

                MinOrderValue = string.IsNullOrEmpty(request.MinOrderValue) ? null : decimal.Parse(request.MinOrderValue),

                PointCost = string.IsNullOrEmpty(request.PointCost) ? null : decimal.Parse(request.PointCost),

                MaxUsagePerUser = request.MaxUsagePerUser == 0 ? null : request.MaxUsagePerUser,

                EndDate = request.EndDate != null ? request.EndDate.ToDateTimeOffset() : null,

                VoucherItems = request.VoucherItems.Select(item => new Application.Dtos.Voucher.UpdateVoucherItemReq()
                {
                    Id = string.IsNullOrEmpty(item.Id) ? null : Guid.Parse(item.Id),
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    Type = (Domain.Constants.VoucherItemType)item.Type
                }).ToList()
            };

            await _mediator.Send(new UpdateVoucherCommand(Request: updateVoucher));
            return new Empty();
        }

        public async override Task<Empty> UpdateVoucherStatus(UpdateVoucherStatusRequest request, ServerCallContext context)
        {

            var updateReq = new Application.Dtos.Voucher.UpdateVoucherStatusRequest()
            {
                Id = Guid.Parse(request.Id),
                IsActive = request.IsActive
            };
            await _mediator.Send(new UpdateVoucherStatusCommand(Request: updateReq));
            return new Empty();
        }

        public async override Task<VoucherListResponse> GetAllVouchers(Empty request, ServerCallContext context)
        {
            var vouchers = await _mediator.Send(new GetAllVoucherQuery());

            var response = new VoucherListResponse();
            response.Vouchers.AddRange(vouchers.Select(v =>
            {
                var voucher = new VoucherResponse()
                {
                    Id = v.Id.ToString(),
                    Code = v.Code,
                    Type = (VoucherType)v.Type,
                    DiscountValue = v.DiscountValue?.ToString() ?? "",
                    MaxDiscountValue = v.MaxDiscountValue?.ToString() ?? "",
                    MinOrderValue = v.MinOrderValue?.ToString() ?? "",
                    PointCost = v.PointCost?.ToString() ?? "",
                    EndDate = v.EndDate != null ? Timestamp.FromDateTimeOffset(v.EndDate.Value) : null,
                    IsActive = v.IsActive,
                    CreatedAt = Timestamp.FromDateTimeOffset(v.CreatedAt),
                    UpdatedAt = Timestamp.FromDateTimeOffset(v.UpdatedAt),
                    VoucherItems = { v.VoucherItems.Select(vi => new VoucherItemResponse()
                    {
                        Id = vi.Id.ToString(),
                        Sku = vi.Sku,
                        Quantity = vi.Quantity,
                        Type = (VoucherItemType)vi.Type,
                    }).ToList()
                    }
                };
                if (v.InitQuantity.HasValue)
                {
                    voucher.InitQuantity = v.InitQuantity.Value;
                    voucher.CurrentQuantity = v.CurrentQuantity ?? 0;
                }

                if (v.MaxUsagePerUser.HasValue)
                {
                    voucher.MaxUsagePerUser = v.MaxUsagePerUser.Value;
                }
                return voucher;
            }).ToList());

            return response;
        }

        public async override Task<VoucherResponse> GetVoucherByCode(GetVoucherByCodeRequest request, ServerCallContext context)
        {
            var voucher = await _mediator.Send(new GetVoucherByCodeQuery(request.Code));
            var response = new VoucherResponse()
            {
                Id = voucher.Id.ToString(),
                Code = voucher.Code,
                Type = (VoucherType)voucher.Type,
                DiscountValue = voucher.DiscountValue?.ToString() ?? "",
                MaxDiscountValue = voucher.MaxDiscountValue?.ToString() ?? "",
                MinOrderValue = voucher.MinOrderValue?.ToString() ?? "",
                EndDate = voucher.EndDate != null ? Timestamp.FromDateTimeOffset(voucher.EndDate.Value) : null,
                VoucherItems = { voucher.VoucherItems.Select(vi => new VoucherItemResponse()
                                {
                                    Id = vi.Id.ToString(),
                                    Sku = vi.Sku,
                                    Quantity = vi.Quantity,
                                    Type = (VoucherItemType)vi.Type,
                                }).ToList()
                }
            };

            return response;
        }

        public async override Task<Empty> DeleteVoucher(DeleteVoucherRequest request, ServerCallContext context)
        {
            await _mediator.Send(new DeleteVoucherCommand(Guid.Parse(request.Id)));
            return new Empty();
        }

        public async override Task<VoucherListResponse> GetMyVouchers(Empty request, ServerCallContext context)
        {
            var userId = context.RequestHeaders.FirstOrDefault(h =>
               h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));

            var vouchers = await _mediator.Send(new GetMyVoucherQuery(Guid.Parse(userId!.Value)));
            var response = new VoucherListResponse();
            response.Vouchers.AddRange(vouchers.Select(v =>
            {
                var voucher = new VoucherResponse()
                {
                    Id = v.Id.ToString(),
                    Code = v.Code,
                    Type = (VoucherType)v.Type,
                    DiscountValue = v.DiscountValue?.ToString() ?? "",
                    MaxDiscountValue = v.MaxDiscountValue?.ToString() ?? "",
                    MinOrderValue = v.MinOrderValue?.ToString() ?? "",
                    PointCost = v.PointCost?.ToString() ?? "",
                    EndDate = v.EndDate != null ? Timestamp.FromDateTimeOffset(v.EndDate.Value) : null,
                    IsActive = v.IsActive,
                    CurrentQuantity = v.CurrentQuantity ?? 0,
                    CreatedAt = Timestamp.FromDateTimeOffset(v.CreatedAt),
                    UpdatedAt = Timestamp.FromDateTimeOffset(v.UpdatedAt),
                    VoucherItems = { v.VoucherItems.Select(vi => new VoucherItemResponse()
                    {
                        Id = vi.Id.ToString(),
                        Sku = vi.Sku,
                        Quantity = vi.Quantity,
                        Type = (VoucherItemType)vi.Type,
                    }).ToList()
                    }
                };

                if (v.MaxUsagePerUser.HasValue)
                {
                    voucher.MaxUsagePerUser = v.MaxUsagePerUser.Value;
                }
                return voucher;
            }).ToList());

            return response;
        }

        public override async Task<Empty> UseVouchers(UseVoucherRequest request, ServerCallContext context)
        {

            var useVoucherQuery = new UseVoucherCommand(new Application.Dtos.Voucher.UseVouchersRequest()
            {
                VoucherIds = request.VoucherIds.Select(id => Guid.Parse(id)).ToList(),
                OrderId = Guid.Parse(request.OrderId),
                UserId = Guid.Parse(request.UserId)
            });

            await _mediator.Send(useVoucherQuery);

            return new Empty();
        }

        public override async Task<Empty> ReverseVouchers(ReverseVouchersRequest request, ServerCallContext context)
        {
            await _mediator.Send(new ReverseVoucherCommand(Guid.Parse(request.OrderId)));
            return new Empty();
        }
    }
}
