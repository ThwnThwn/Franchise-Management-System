using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Commands.Create;
using EngagementService.Application.Features.Campaign.Commands.RedeemCampaignVoucher;
using EngagementService.Application.Features.Campaign.Commands.UpdateActiveStatus;
using EngagementService.Application.Features.Campaign.Queries.GetActive;
using EngagementService.Application.Features.Campaign.Queries.GetAll;
using EngagementService.Application.Features.Campaign.Queries.GetById;
using EngagementService.Grpc.Protos.CampaignGrpc;
using Grpc.Core;
using MediatR;

namespace EngagementService.API.Services.Campagin
{
    public class CampaignGrpcClient : CampaignGrpc.CampaignGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<CampaignGrpcClient> _logger;

        public CampaignGrpcClient(IMediator mediator, IMapper mapper, ILogger<CampaignGrpcClient> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<CampaignResponse> CreateCampaign(CreateCampaignRequest request, ServerCallContext context)
        {
            //Map gRPC - Command
            var command = new CreateCampaginCommand(new CreateCampaginReq
            {
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate.ToDateTimeOffset(),
                EndDate = request.EndDate.ToDateTimeOffset(),
                VoucherIds = request.VoucherIds.Select(Guid.Parse).ToList()
            });
            var result = await _mediator.Send(command);

            //Result - gRPC Response
            var response = MapCampaignToGrpc(result);
            return response;
        }

        public override async Task<CampaignResponse> UpdateActiveStatus(UpdateActiveStatusRequest request, ServerCallContext context)
        {
            var command = new UpdateActiveStatusCommand(new UpdateActiveStatusReq
            {
                Id = Guid.Parse(request.Id),
                IsActive = request.IsActive,
            });
            var result = await _mediator.Send(command);

            //Result - gRPC Response
            var response = MapCampaignToGrpc(result);
            return response;
        }

        public override async Task<CampaignResponse> RedeemCampainVocuher(RedeemCampainVoucherRequest request, ServerCallContext context)
        {
            var command = new RedeemCampaignVoucherCommand(new RedeemCampaignVoucherReq
            {
                UserId = Guid.Parse(request.UserId),
                VoucherId = Guid.Parse(request.VoucherId)
            });
            var result = await _mediator.Send(command);

            //Result - gRPC Response
            var response = MapCampaignToGrpc(result);
            return response;
        }

        public override async Task<CampaignResponse> GetById(GetByIdRequest request, ServerCallContext context)
        {
            var query = new GetByIdQuery(Guid.Parse(request.Id));

            var result = await _mediator.Send(query);
            //Result - gRPC Response
            var response = MapCampaignToGrpc(result);
            return response;
        }

        public override async Task<ListCampaignResponse> GetAll(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            var result = await _mediator.Send(new GetAllQuery());

            var response = new ListCampaignResponse();
            response.Campaigns.AddRange(
                result.Select(MapCampaignToGrpc)
            );

            return response;
        }

        public override async Task<ListCampaignResponse> GetActive(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            var result = await _mediator.Send(new GetActiveQuery());
            var response = new ListCampaignResponse();
            response.Campaigns.AddRange(
                result.Select(MapCampaignToGrpc)
            );
            return response;
        }

        //===================================
        ////=========HELPER METHOD===========
        //===================================
        public CampaignResponse MapCampaignToGrpc(CampaignRes result)
        {
            var response = new CampaignResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                Description = result.Description ?? "",
                StartDate = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.StartDate),
                EndDate = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.EndDate),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.CreatedAt),
                UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.UpdatedAt),
                IsActive = result.IsActive ?? false,
            };
            //Map vouchers
            response.Vouchers.AddRange(result.Vouchers.Select(v => new Grpc.Protos.VoucherGrpc.VoucherResponse
            {
                Id = v.Id.ToString(),
                Code = v.Code,
                Type = (Grpc.Protos.VoucherGrpc.VoucherType)v.Type,
                DiscountValue = v.DiscountValue?.ToString() ?? "",
                MaxDiscountValue = v.MaxDiscountValue?.ToString() ?? "",
                MinOrderValue = v.MinOrderValue?.ToString() ?? "",
                PointCost = v.PointCost?.ToString() ?? "",
                InitQuantity = v.InitQuantity ?? 0,
                CurrentQuantity = v.CurrentQuantity ?? 0,
                MaxUsagePerUser = v.MaxUsagePerUser ?? 0,
                IsActive = v.IsActive,
                EndDate = v.EndDate.HasValue
                    ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v.EndDate.Value)
                    : null,
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(v.CreatedAt),
                UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(v.UpdatedAt),
                VoucherItems =
                {
                    v.VoucherItems.Select(vi => new Grpc.Protos.VoucherGrpc.VoucherItemResponse
                    {
                        Id = vi.Id.ToString(),
                        Type = (Grpc.Protos.VoucherGrpc.VoucherItemType)vi.Type,
                        Quantity = vi.Quantity ,
                        Sku = vi.Sku
                    })
                },
            }));
            return response;
        }
    }
}