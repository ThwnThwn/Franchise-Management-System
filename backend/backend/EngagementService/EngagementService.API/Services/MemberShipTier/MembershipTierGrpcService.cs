using EngagementService.Grpc.Protos.MembershipTier;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace EngagementService.API.Services.MemberShipTier
{
    public class MembershipTierGrpcService : MembershipTierGrpc.MembershipTierGrpcBase
    {
        private readonly IMediator _mediator;
        public MembershipTierGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async override Task<Empty> CreateMembershipTier(CreateMembershipTierRequest request, ServerCallContext context)
        {
            var createReq = new Application.Dtos.MembershipTier.CreateMembershipTierRequest();
            try
            {
                createReq = new Application.Dtos.MembershipTier.CreateMembershipTierRequest
                {
                    Name = request.Name,
                    MinSpendRequired = decimal.Parse(request.MinSpendRequired),
                    DiscountRate = request.DiscountRate,
                    ValuePerPoint = decimal.Parse(request.ValuePerPoint),
                    AmountPerPoint = decimal.Parse(request.AmountPerPoint)
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid input: {ex.Message}"));
            }

            var command = new Application.Features.MembershipTier.Commands.Create.CreateMembershipTierCommand
            (Request: createReq);
            await _mediator.Send(command); // Add await for async call
            return new Empty();
        }

        public async override Task<Empty> UpdateMembershipTier(UpdateMembershipTierRequest request, ServerCallContext context)
        {

            var updateReq = new Application.Dtos.MembershipTier.UpdateMembershipTierRequest();
            try
            {
                updateReq = new Application.Dtos.MembershipTier.UpdateMembershipTierRequest
                {
                    Id = Guid.Parse(request.Id),
                    Name = request.Name,
                    MinSpendRequired = string.IsNullOrEmpty(request.MinSpendRequired) ? (decimal?)null : decimal.Parse(request.MinSpendRequired),
                    DiscountRate = request.HasDiscountRate ? request.DiscountRate : (double?)null,
                    ValuePerPoint = string.IsNullOrEmpty(request.ValuePerPoint) ? (decimal?)null : decimal.Parse(request.ValuePerPoint),
                    AmountPerPoint = string.IsNullOrEmpty(request.AmountPerPoint) ? (decimal?)null : decimal.Parse(request.AmountPerPoint)
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid input: {ex.Message}"));
            }
            var command = new Application.Features.MembershipTier.Commands.Update.UpdateMembershipTierCommand
            (Request: updateReq);
            await _mediator.Send(command); // Add await for async call
            return new Empty();
        }

        // 1. Sửa kiểu trả về thành Task<MembershipTierListResponse>
        public async override Task<MembershipTierListResponse> GetAllMembershipTiers(Empty request, ServerCallContext context)
        {
            var query = new Application.Features.MembershipTier.Queries.GetAll.GetAllMembershipTierQuery();
            var result = await _mediator.Send(query);

            // 2. Khởi tạo đối tượng Response bao bọc (Wrapper)
            var response = new MembershipTierListResponse();

            response.Tiers.AddRange(result.Select(t => new MembershipTierResponse
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                MinSpendRequired = t.MinSpendRequired.ToString(), // Chuyển decimal sang string để gRPC hiểu
                DiscountRate = t.DiscountRate,
                ValuePerPoint = t.ValuePerPoint.ToString(),
                AmountPerPoint = t.AmountPerPoint.ToString()
            }));

            return response;
        }

        public async override Task<Empty> DeleteMembershipTier(DeleteMembershipTierRequest request, ServerCallContext context)
        {
            var id = Guid.TryParse(request.Id, out var parsedId) ? parsedId : Guid.Empty;
            var command = new Application.Features.MembershipTier.Commands.Delete.DeleteMembershipTierCommand
            (Id: id);
            await _mediator.Send(command); // Add await for async call
            return new Empty();
        }
    }
}
