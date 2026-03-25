using EngagementService.Grpc.Protos.LoyaltyAccountGrpc;
using EngagementService.Grpc.Protos.VoucherGrpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.ClientInterface.Engagement;
using SalesService.Application.Dtos.Voucher;
using static EngagementService.Grpc.Protos.LoyaltyAccountGrpc.LoyaltyAccountGrpc;
using static EngagementService.Grpc.Protos.VoucherGrpc.VoucherGrpc;

namespace SalesService.Infrastructure.ExternalServices
{
    public class EngagementSeviceClient : IEngagementServiceClient
    {
        private readonly VoucherGrpc.VoucherGrpcClient _voucherClient;
        private readonly LoyaltyAccountGrpc.LoyaltyAccountGrpcClient _accountClient;
        private readonly ILogger<VoucherGrpcClient> _voucherLogger;
        private readonly ILogger<LoyaltyAccountGrpcClient> _accountLogger;

        public EngagementSeviceClient(IConfiguration configuration, ILogger<VoucherGrpcClient> voucherLogger, ILogger<LoyaltyAccountGrpcClient> accountLogger)
        {
            var engagementServiceUrl = configuration["ENGAGEMENT_SERVICE_URL"] ?? "http://localhost:5007";
            var channel = GrpcChannel.ForAddress(engagementServiceUrl);
            _voucherClient = new VoucherGrpc.VoucherGrpcClient(channel);
            _accountClient = new LoyaltyAccountGrpc.LoyaltyAccountGrpcClient(channel);
            _voucherLogger = voucherLogger;
            _accountLogger = accountLogger;

        }

        public async Task<decimal> GetMyPointByEmailAsync(string email)
        {
            try
            {
                var request = new GetByUserPhoneOrEmailRequest
                {
                    Email = email
                };

                var loyalty = await _accountClient.GetByUserPhoneOrEmailAsync(request);
                string pointsStr = loyalty?.AccountsRes?.FirstOrDefault()?.AvailablePoints ?? "0";

                // Use TryParse to avoid crashes if the string is malformed
                if (!decimal.TryParse(pointsStr, out decimal point))
                {
                    point = 0;
                }
                return point;

            }
            catch (Exception ex)
            {
                _accountLogger.LogError(ex, "Failed to get user point {email} from engagement", email);
                return 0;
            }
        }

        public async Task<IEnumerable<Voucher>> GetMyVoucherAsync(Guid userId)
        {
            IEnumerable<Voucher> vouchers = new List<Voucher>();
            try
            {
                var metadata = new Grpc.Core.Metadata
                {
                    { "x-user-id", userId.ToString() }
                };
                var request = new Empty();
                var response = await _voucherClient.GetMyVouchersAsync(request, metadata);

                vouchers = response.Vouchers.Select(v => new Voucher
                {
                    Id = Guid.Parse(v.Id),
                    Code = v.Code,
                    Type = (Application.Dtos.Voucher.VoucherType)v.Type,
                    DiscountValue = string.IsNullOrEmpty(v.DiscountValue) ? null : decimal.Parse(v.DiscountValue),
                    MaxDiscountValue = string.IsNullOrEmpty(v.MaxDiscountValue) ? null : decimal.Parse(v.MaxDiscountValue),
                    MinOrderValue = string.IsNullOrEmpty(v.MinOrderValue) ? null : decimal.Parse(v.MinOrderValue),
                    PointCost = string.IsNullOrEmpty(v.PointCost) ? null : decimal.Parse(v.PointCost),
                    InitQuantity = v.InitQuantity,
                    CurrentQuantity = v.CurrentQuantity,
                    MaxUsagePerUser = v.MaxUsagePerUser,
                    IsActive = v.IsActive,
                    EndDate = v.EndDate?.ToDateTimeOffset(),
                    CreatedAt = v.CreatedAt.ToDateTimeOffset(),
                    UpdatedAt = v.UpdatedAt.ToDateTimeOffset(),

                    VoucherItems = v.VoucherItems.Select(vi => new VoucherItem
                    {
                        Id = Guid.Parse(vi.Id),
                        VoucherId = Guid.Parse(v.Id),
                        Sku = vi.Sku,
                        Quantity = vi.Quantity,
                        Type = (Application.Dtos.Voucher.VoucherItemType)vi.Type
                    }).ToList()
                }).ToList();

                return vouchers;
            }
            catch (Exception ex)
            {
                _voucherLogger.LogError(ex, "Failed to get user voucher {UserId} from engagement", userId);
                return vouchers;
            }
        }
    }
}
