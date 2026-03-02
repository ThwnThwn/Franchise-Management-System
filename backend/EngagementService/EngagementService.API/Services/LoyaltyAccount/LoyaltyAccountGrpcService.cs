using EngagementService.Application.Abstraction;
using EngagementService.Application.Constants;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetById;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetByIdsFilter;
using EngagementService.Application.Features.LoyaltyAccount.Queries.GetLoyaltyReport;
using EngagementService.Grpc.Protos.LoyaltyAccountGrpc;
using EngagementService.Grpc.Protos.MembershipTier;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace EngagementService.API.Services.LoyaltyAccount
{
    public class LoyaltyAccountGrpcService : LoyaltyAccountGrpc.LoyaltyAccountGrpcBase
    {
        private readonly IMediator _mediator;
        private readonly IReportService _reportService;
        public LoyaltyAccountGrpcService(IMediator mediator, IReportService reportService)
        {
            _mediator = mediator;
            _reportService = reportService;
        }

        public override async Task<Empty> ProcessOrderReward(ProcessOrderRewardRequest request, ServerCallContext context)
        {
            var id = Guid.Parse(request.Id);
            var checkReq = new GetByIdQuery(Id: id);

            var account = await _mediator.Send(checkReq);
            var upsertRequest = new Application.Dtos.LoyaltyAccount.UpSertReqest
            {
                Id = id,
                OrderAmount = int.Parse(request.OrderAmount),
                OrderId = Guid.Parse(request.OrderId)
            };
            if (account == null)
            {
                // chưa có tài khoản thì tạo mới rồi mới tính điểm
                var createCommand = new Application.Features.LoyaltyAccount.Commands.Create.CreateCommand
                (upsertRequest);
                await _mediator.Send(createCommand);
                return new Empty();
            }
            var processCommand = new Application.Features.LoyaltyAccount.Commands.ProcessOrderReward.ProcessOrderRewardCommand
               (upsertRequest);
            await _mediator.Send(processCommand);
            return new Empty();
        }

        public override async Task<LoyaltyAccountsResponse> GetByUserPhoneOrEmail(GetByUserPhoneOrEmailRequest request, ServerCallContext context)
        {
            var req = new Application.Dtos.LoyaltyAccount.GetByUserPhoneOrEmailRequest()
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var accounts = await _mediator.Send(new GetByPhoneOrEmailQuery(req));

            var response = new LoyaltyAccountsResponse();
            foreach (var account in accounts)
            {
                //List<LogResponse> logResponse = new();
                //if (account.PointLogs != null)
                //{
                //    logResponse.AddRange(account.PointLogs.Select(log => new LogResponse
                //    {
                //        Id = log.Id.ToString(),
                //        UserId = log.UserId.ToString(),
                //        Amount = log.Amount.ToString(),
                //        BalanceAfter = log.BalanceAfter.ToString(),
                //        BalanceBefore = log.BalanceBefore.ToString(),
                //        ActionType = log.ActionType.ToString(),
                //        ReasonCode = log.ReasonCode.ToString(),
                //        Description = log.Description,
                //        ReferenceId = log.ReferenceId?.ToString() ?? string.Empty,
                //        ExpiredAt = log.ExpiredAt?.ToString("o") ?? string.Empty
                //    }));
                //}

                var tierRep = new MembershipTierResponse()
                {
                    Id = account.CurrentTier.Id.ToString(),
                    Name = account.CurrentTier.Name,
                    MinSpendRequired = account.CurrentTier.MinSpendRequired.ToString(), // Chuyển decimal sang string để gRPC hiểu
                    DiscountRate = account.CurrentTier.DiscountRate,
                    ValuePerPoint = account.CurrentTier.ValuePerPoint.ToString(),
                    AmountPerPoint = account.CurrentTier.AmountPerPoint.ToString()
                };


                var responseItem = new LoyaltyAccountResponse
                {
                    Id = account.Id.ToString(),
                    UserName = account.UserName,
                    AvailablePoints = account.AvailablePoints.ToString(),
                    TotalSpent = account.TotalSpent.ToString(),
                    CurrentTier = tierRep,
                    //LogRep = { logResponse }
                };

                response.AccountsRes.Add(responseItem);
            }

            return response;
        }

        public async override Task<Empty> CreateLoyaltyAccount(CreateLoyaltyAccountRequest request, ServerCallContext context)
        {
            var upSertReq = new Application.Dtos.LoyaltyAccount.UpSertReqest
            {
                Id = Guid.Parse(request.UserId),
                OrderAmount = 0,
                OrderId = Guid.Empty
            };
            await _mediator.Send(new Application.Features.LoyaltyAccount.Commands.Create.CreateCommand(upSertReq));

            return new Empty();
        }

        public async override Task<Empty> DeductPoints(DeductPointRequest request, ServerCallContext context)
        {
            var req = new Application.Dtos.LoyaltyAccount.DeductPointRequest
            {
                Id = Guid.Parse(request.Id),
                PointToDeduct = decimal.Parse(request.PointToDeduct),
                OrderId = Guid.Parse(request.OrderId)
            };
            await _mediator.Send(new Application.Features.LoyaltyAccount.Commands.DetuctPoint.DeductPointCommand(req));
            return new Empty();
        }

        public override async Task<Grpc.Protos.LoyaltyAccountGrpc.LoyaltyReportResponse> GetLoyaltyReport(GetLoyaltyReportRequest request, ServerCallContext context)
        {
            // 1. Ánh xạ từ gRPC Request sang MediatR Query
            var query = new GetLoyaltyReportQuery(
                FromDate: request.FromDate?.ToDateTime(),
                ToDate: request.ToDate?.ToDateTime(),
                TierId: request.TierId,
                Format: (Application.Constants.ExportFormat)request.Format
            );

            // 2. Gửi Query vào Application Layer thông qua MediatR để lấy dữ liệu thống kê
            var result = await _mediator.Send(query);

            // 3. Khởi tạo Response khớp với định nghĩa proto
            var response = new Grpc.Protos.LoyaltyAccountGrpc.LoyaltyReportResponse
            {
                TotalMembers = result.Data.TotalMembers,
                TotalPointsEarned = (double)result.Data.TotalPointsEarned,
                TotalPointsRedeemed = (double)result.Data.TotalPointsRedeemed
            };

            // 4. Ánh xạ danh sách TierDistribution
            if (result.Data.TierDistribution != null)
            {
                var distributions = result.Data.TierDistribution.Select(d => new TierDistribution
                {
                    TierName = d.TierName,
                    MemberCount = d.MemberCount
                });

                response.TierDistribution.AddRange(distributions);
            }

            // 5. Logic xuất file vật lý (PDF/CSV) trả về qua gRPC
            // Kiểm tra định dạng yêu cầu từ request.Format
            if (request.Format == Grpc.Protos.LoyaltyAccountGrpc.ExportFormat.Pdf)
            {
                // Gọi ReportService để vẽ PDF và chuyển thành ByteString
                var pdfBytes = _reportService.ExportToPdf(result.Data);
                response.FileContent = Google.Protobuf.ByteString.CopyFrom(pdfBytes);
            }
            else if (request.Format == Grpc.Protos.LoyaltyAccountGrpc.ExportFormat.Csv)
            {
                // Gọi ReportService để xuất CSV và chuyển thành ByteString
                var csvBytes = _reportService.ExportToCsv(result.Data);
                response.FileContent = Google.Protobuf.ByteString.CopyFrom(csvBytes);
            }

            return response;
        }

        public override async Task<LoyaltyAccountsResponse> GetByUserIdsWithFilter(GetByUserIdsWithFilterRequest request,ServerCallContext context)
        {
            // ===== 1️⃣ Parse filters safely =====
            var userIds = request.Ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(Guid.Parse)
                .ToList();
            Guid? tierId = string.IsNullOrEmpty(request.TierId)
                ? null
                : Guid.Parse(request.TierId);
            decimal? startAt = (decimal)(request.StartAt);
            decimal? endAt = (decimal)(request.EndAt);
            var query = new GetByIdsFilterQuery
            {
                UserIds = userIds,
                TierId = tierId,
                StartAt = startAt,
                EndAt = endAt
            };
            var accounts = _mediator.Send(query).Result;
            var response = new LoyaltyAccountsResponse();

            foreach (var account in accounts)
            {
                var tierRep = new MembershipTierResponse()
                {
                    Id = account.CurrentTier.Id.ToString(),
                    Name = account.CurrentTier.Name,
                    MinSpendRequired = account.CurrentTier.MinSpendRequired.ToString(), // Chuyển decimal sang string để gRPC hiểu
                    DiscountRate = account.CurrentTier.DiscountRate,
                    ValuePerPoint = account.CurrentTier.ValuePerPoint.ToString(),
                    AmountPerPoint = account.CurrentTier.AmountPerPoint.ToString()
                };
                response.AccountsRes.Add(new LoyaltyAccountResponse
                {
                    Id = account.Id.ToString(),
                    AvailablePoints = account.AvailablePoints.ToString(),
                    TotalSpent = account.TotalSpent.ToString(),
                    CurrentTier = tierRep,
                });
            }

            return response;
        }
    }
}
