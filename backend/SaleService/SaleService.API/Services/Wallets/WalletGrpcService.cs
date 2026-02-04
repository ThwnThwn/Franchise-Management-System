
using Confluent.Kafka;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.Wallets.Command.AddBalance;
using SalesService.Application.Features.Wallets.Command.Create;
using SalesService.Application.Features.Wallets.Command.DeductBalance;
using SalesService.Application.Features.Wallets.Commands.UpdateStatus;
using SalesService.Application.Features.Wallets.Queries.GetByUserId;
using SalesService.Grpc;
using DomainWalletStatus = SalesService.Domain.Enums.WalletStatus;
using GrpcWalletStatus = SalesService.Grpc.WalletStatus;

namespace SalesService.API.Services.Wallets
{
    public sealed class WalletGrpcService : WalletGrpc.WalletGrpcBase
    {
        private readonly IMediator _mediator;

        public WalletGrpcService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override async Task<CreateWalletResponse> CreateWallet(
            CreateWalletRequest request,
            ServerCallContext context)
        {
            var command = new CreateWalletCommand
            {
                UserId = Guid.Parse(request.UserId)
            };

            var walletId = await _mediator.Send(command);

            return new CreateWalletResponse
            {
                WalletId = walletId.ToString()
            };
        }

        public override async Task<AddBalanceResponse> AddBalance(
        AddBalanceRequest request,
        ServerCallContext context)
        {
            var command = new AddBalanceCommand(Guid.Parse(request.WalletId),
                new AddBalanceCommand.AddBalanceCommandRequest
                {
                    Amount = (decimal)request.Amount,
                    RowVersion = request.RowVersion.ToByteArray()
                });

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                return new AddBalanceResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault() ?? "Failed to add balance."
                };
            }

            var data = result.Value;

            return new AddBalanceResponse
            {
                Success = true,
                Message = "Balance added successfully.",

                WalletId = data.WalletId.ToString(),
                UserId = data.UserId.ToString(),

                BalanceBefore = (double)data.BalanceBefore,
                BalanceAfter = (double)data.BalanceAfter,
                AmountAdded = (double)data.AmountAdded,

                TransactionId = data.TransactionId?.ToString() ?? string.Empty,
                NewRowVersion = Google.Protobuf.ByteString.CopyFrom(data.NewRowVersion),

                UpdatedAt = data.UpdatedAt.ToString("O")
            };
        }
        
        public override async Task<DeductBalanceResponse> DeductBalance(
        DeductBalanceRequest request,
        ServerCallContext context)
        {
            var command = new DeductBalanceCommand(Guid.Parse(request.WalletId),
                new DeductBalanceCommand.DeductBalanceCommandRequest
                {
                    Amount = (decimal)request.Amount,
                    RowVersion = request.RowVersion.ToByteArray()
                });

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                return new DeductBalanceResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault() ?? "Failed to deduct balance."
                };
            }

            var data = result.Value;

            return new DeductBalanceResponse
            {
                Success = true,
                Message = "Balance deducted successfully.",

                WalletId = data.WalletId.ToString(),
                UserId = data.UserId.ToString(),

                BalanceBefore = (double)data.BalanceBefore,
                BalanceAfter = (double)data.BalanceAfter,
                AmountDeducted = (double)data.AmountDeducted,

                TransactionId = data.TransactionId?.ToString() ?? string.Empty,
                NewRowVersion = Google.Protobuf.ByteString.CopyFrom(data.NewRowVersion),

                UpdatedAt = data.UpdatedAt.ToString("O")
            };
        }

        public override async Task<UpdateWalletStatusResponse> UpdateWalletStatus(
        UpdateWalletStatusRequest request,
        ServerCallContext context)
        {
            var command = new UpdateWalletStatusCommand(Guid.Parse(request.WalletId),
                new UpdateWalletStatusCommand.UpdateWalletStatusRequest
                {
                    NewStatus = (DomainWalletStatus)request.NewStatus,
                    Reason = string.IsNullOrWhiteSpace(request.Reason)
                        ? null
                        : request.Reason
                });

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        string.Join("; ", result.Errors)
                    )
                );
            }

            var data = result.Value;

            return new UpdateWalletStatusResponse
            {
                Success = true,
                Message = "Balance Status updated successfully.",
                WalletId = data.WalletId.ToString(),
                OldStatus = (GrpcWalletStatus)data.OldStatus,
                NewStatus = (GrpcWalletStatus)data.NewStatus,
                UpdatedAt = data.UpdatedAt.ToString("O")
            };
        }

        public override async Task<GetWalletByUserIdResponse> GetWalletByUserId(
        GetWalletByUserIdRequest request,
        ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Invalid user id"));
            }

            var result = await _mediator.Send(
                new GetWalletByUserIdQuery(userId),
                context.CancellationToken);

            if (result.Status == Ardalis.Result.ResultStatus.NotFound)
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, result.Errors.First()));
            }

            var wallet = result.Value;

            return new GetWalletByUserIdResponse
            {
                Id = wallet.Id.ToString(),
                UserId = wallet.UserId.ToString(),
                Balance = (double)wallet.Balance,
                Status = wallet.Status,
                RowVersion = wallet.RowVersion is null
                    ? Google.Protobuf.ByteString.Empty
                    : Google.Protobuf.ByteString.CopyFrom(wallet.RowVersion),
                CreatedAt = wallet.CreatedAt.ToString("O"),
                UpdatedAt = wallet.UpdatedAt.ToString("O")
            };
        }
    }
}

