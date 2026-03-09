using EngagementService.Grpc.Protos.VoucherGrpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesService.Application.Dtos.Voucher;
using SalesService.Application.ExternalServices;
using static EngagementService.Grpc.Protos.VoucherGrpc.VoucherGrpc;

namespace SalesService.Infrastructure.ExternalServices
{
    public class EngagementSeviceClient : IEngagementServiceClient
    {
        private readonly VoucherGrpc.VoucherGrpcClient _client;
        private readonly ILogger<VoucherGrpcClient> _logger;

        public EngagementSeviceClient(IConfiguration configuration, ILogger<VoucherGrpcClient> logger)
        {
            var identityServiceUrl = configuration["ENGAGEMENT_SERVICE_URL"] ?? "http://localhost:5007";
            var channel = GrpcChannel.ForAddress(identityServiceUrl);
            _client = new VoucherGrpc.VoucherGrpcClient(channel);
            _logger = logger;
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
                var response = await _client.GetMyVouchersAsync(request, metadata);

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
                        VariantId = Guid.Parse(vi.VariantId),
                        Quantity = vi.Quantity,
                        Type = (Application.Dtos.Voucher.VoucherItemType)vi.Type
                    }).ToList()
                }).ToList();

                return vouchers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user voucher {UserId} from IdentityService", userId);
                return vouchers;
            }
        }
    }
}
