using AutoMapper;
using EngagementService.Application.Dtos.Campaign.Request;
using EngagementService.Application.Dtos.Campaign.Response;
using EngagementService.Application.Features.Campaign.Commands.Create;
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
                StartDate = request.StartDate.ToDateTime(),
                EndDate = request.EndDate.ToDateTime(),
                VoucherIds = request.VoucherIds.Select(Guid.Parse).ToList()
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

        //===================================
        ////=========HELPER METHOD===========
        //===================================
        public CampaignResponse MapCampaignToGrpc(CampaginRes result)
        {
            var response = new CampaignResponse
            {
                Id = result.Id.ToString(),
                Name = result.Name,
                Description = result.Description ?? "",
                StartDate = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTime(result.StartDate.ToUniversalTime()),
                EndDate = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTime(result.EndDate.ToUniversalTime()),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.CreatedAt),
                UpdatedAt = Google.Protobuf.WellKnownTypes.Timestamp
                    .FromDateTimeOffset(result.UpdatedAt),
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