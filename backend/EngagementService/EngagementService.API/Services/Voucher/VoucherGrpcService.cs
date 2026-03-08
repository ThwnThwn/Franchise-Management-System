using EngagementService.Application.Features.Voucher.Commands.Create;
using EngagementService.Application.Features.Voucher.Commands.Delete;
using EngagementService.Application.Features.Voucher.Commands.Update;
using EngagementService.Application.Features.Voucher.Commands.UpdateVoucherStatus;
using EngagementService.Application.Features.Voucher.Queries.GetAll;
using EngagementService.Application.Features.Voucher.Queries.GetByCode;
using EngagementService.Application.Features.Voucher.Queries.GetMyVoucher;
using EngagementService.Domain.Constants;
using EngagementService.Infrastructure.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using System.Runtime.CompilerServices;

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
            var roleEntry = (context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase)));

            if (roleEntry == null || roleEntry.Value != "admin")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Bạn không có quyền thực hiện hành động này"));
            }

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
                    VariantId = Guid.Parse(item.VariantId),
                    Quantity = item.Quantity,
                    Type = (Domain.Constants.VoucherItemType)item.Type
                }).ToList()
            };

            await _mediator.Send(new CreateVoucherCommand(Request: createReq));
            return new Empty();
        }

        public async override Task<Empty> UpdateVoucher(UpdateVoucherRequest request, ServerCallContext context)
        {
            var roleEntry = (context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase)));

            if(roleEntry == null || roleEntry.Value != "admin")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Bạn không có quyền thực hiện hành động này"));
            }
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
                    VariantId = Guid.Parse(item.VariantId),
                    Quantity = item.Quantity,
                    Type = (Domain.Constants.VoucherItemType)item.Type
                }).ToList()
            };

            await _mediator.Send(new UpdateVoucherCommand(Request: updateVoucher));
            return new Empty();
        }

        public async override Task<Empty> UpdateVoucherStatus(UpdateVoucherStatusRequest request, ServerCallContext context)
        {
            var roleEntry = (context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase)));

            if (roleEntry == null || roleEntry.Value != "admin")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Bạn không có quyền thực hiện hành động này"));
            }

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
        
            IEnumerable<Domain.Entities.Voucher> vouchers = new List<Domain.Entities.Voucher>();

            var roleEntry = (context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase)));

            if (roleEntry != null && roleEntry.Value == "admin")
            {
                vouchers = await _mediator.Send(new GetAllVoucherQuery());
            }
            else
            {
                var userId = context.RequestHeaders.FirstOrDefault(h =>
                h.Key.Equals("x-user-id", StringComparison.OrdinalIgnoreCase));
                if (userId != null)
                {
                    vouchers = await _mediator.Send(new GetMyVoucherQuery(Guid.Parse(userId.Value)));
                }
            }

            var response = new VoucherListResponse();
            response.Vouchers.AddRange(vouchers.Select(v => new VoucherResponse()
            {
                Id = v.Id.ToString(),
                Code = v.Code,
                Type = (Infrastructure.Protos.VoucherType)v.Type,
                DiscountValue = v.DiscountValue?.ToString() ?? "",
                MaxDiscountValue = v.MaxDiscountValue?.ToString() ?? "",
                MinOrderValue = v.MinOrderValue?.ToString() ?? "",
                PointCost = v.PointCost?.ToString() ?? "",
                InitQuantity = v.InitQuantity ?? 0,
                MaxUsagePerUser = v.MaxUsagePerUser ?? 0,
                EndDate = v.EndDate != null ? Timestamp.FromDateTimeOffset(v.EndDate.Value) : null,
                IsActive = v.IsActive,
                CurrentQuantity = v.CurrentQuantity ?? 0,
                CreatedAt = Timestamp.FromDateTimeOffset(v.CreatedAt),
                UpdatedAt = Timestamp.FromDateTimeOffset(v.UpdatedAt),
                VoucherItems = { v.VoucherItems.Select(vi => new VoucherItemResponse()
                                {
                                    Id = vi.Id.ToString(),
                                    VariantId = vi.VariantId.ToString(),
                                    Quantity = vi.Quantity,
                                    Type = (Infrastructure.Protos.VoucherItemType)vi.Type,
                                }).ToList()
                }
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
                Type = (Infrastructure.Protos.VoucherType)voucher.Type,
                DiscountValue = voucher.DiscountValue?.ToString() ?? "",
                MaxDiscountValue = voucher.MaxDiscountValue?.ToString() ?? "",
                MinOrderValue = voucher.MinOrderValue?.ToString() ?? "",
                EndDate = voucher.EndDate != null ? Timestamp.FromDateTimeOffset(voucher.EndDate.Value) : null,
                VoucherItems = { voucher.VoucherItems.Select(vi => new VoucherItemResponse()
                                {
                                    Id = vi.Id.ToString(),
                                    VariantId = vi.VariantId.ToString(),
                                    Quantity = vi.Quantity,
                                    Type = (Infrastructure.Protos.VoucherItemType)vi.Type,
                                }).ToList()
                }
            };

            return response;
        }

        public async override Task<Empty> DeleteVoucher(DeleteVoucherRequest request, ServerCallContext context)
        {
            var roleEntry = (context.RequestHeaders.FirstOrDefault(h =>
            h.Key.Equals("x-role-slug", StringComparison.OrdinalIgnoreCase)));

            if (roleEntry == null || roleEntry.Value != "admin")
            {
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Bạn không có quyền thực hiện hành động này"));
            }

            await _mediator.Send(new DeleteVoucherCommand(Guid.Parse(request.Id)));
            return new Empty(); 
        }
    }
}
