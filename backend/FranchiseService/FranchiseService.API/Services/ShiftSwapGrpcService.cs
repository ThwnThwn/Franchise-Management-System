using FranchiseService.Grpc;
using FranchiseService.Application.Features.ShiftSwaps.Commands.RequestSwap;
using FranchiseService.Application.Features.ShiftSwaps.Commands.RespondSwap;
using FranchiseService.Application.Features.ShiftSwaps.Commands.ApproveSwap;
using FranchiseService.Application.Features.ShiftSwaps.Commands.CancelSwap;
using FranchiseService.Application.Features.ShiftSwaps.Commands.ReleaseShift;
using FranchiseService.Application.Features.ShiftSwaps.Queries.GetMySwapRequests;
using Grpc.Core;
using MediatR;

namespace FranchiseService.API.Services;

/// <summary>
/// gRPC Service quan ly doi ca va nha ca.
/// Chuyen doi giua gRPC request va MediatR command/query.
/// </summary>
public class ShiftSwapGrpcService
    : Grpc.ShiftSwapGrpcService.ShiftSwapGrpcServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ShiftSwapGrpcService> _logger;

    public ShiftSwapGrpcService(IMediator mediator, ILogger<ShiftSwapGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// A gui yeu cau doi ca cho B
    /// </summary>
    public override async Task<RequestSwapResponse> RequestSwap(
        RequestSwapRequest request, ServerCallContext context)
    {
        try
        {
            var command = new RequestSwapCommand
            {
                AssignmentId = Guid.Parse(request.AssignmentId),
                SourceUserId = Guid.Parse(request.SourceUserId),
                TargetUserId = Guid.Parse(request.TargetUserId),
                Reason = request.Reason
            };

            var result = await _mediator.Send(command);

            return new RequestSwapResponse
            {
                Success = true,
                SwapRequestId = result.SwapRequestId.ToString(),
                ExpiresAt = result.ExpiresAt.ToString("o"),
                Message = "Da gui yeu cau doi ca. Cho nguoi nhan xac nhan."
            };
        }
        catch (FluentValidation.ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// B dong y hoac tu choi yeu cau doi ca
    /// </summary>
    public override async Task<RespondSwapResponse> RespondSwap(
        RespondSwapRequest request, ServerCallContext context)
    {
        try
        {
            var command = new RespondSwapCommand
            {
                SwapRequestId = Guid.Parse(request.SwapRequestId),
                UserId = Guid.Parse(request.UserId),
                Accept = request.Accept,
                RejectReason = string.IsNullOrEmpty(request.RejectReason)
                    ? null : request.RejectReason
            };

            var accepted = await _mediator.Send(command);
            string message = accepted
                ? "Da dong y. Cho Manager duyet."
                : "Da tu choi yeu cau doi ca.";

            return new RespondSwapResponse { Success = true, Message = message };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// Manager duyet hoac tu choi yeu cau doi ca
    /// </summary>
    public override async Task<ApproveSwapResponse> ApproveSwap(
        ApproveSwapRequest request, ServerCallContext context)
    {
        try
        {
            var command = new ApproveSwapCommand
            {
                SwapRequestId = Guid.Parse(request.SwapRequestId),
                ReviewedBy = Guid.Parse(request.ReviewedBy),
                Approve = request.Approve,
                RejectReason = string.IsNullOrEmpty(request.RejectReason)
                    ? null : request.RejectReason
            };

            var approved = await _mediator.Send(command);
            string message = approved
                ? "Da duyet. Ca da duoc doi thanh cong."
                : "Da tu choi yeu cau doi ca.";

            return new ApproveSwapResponse { Success = true, Message = message };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// A tu huy yeu cau doi ca
    /// </summary>
    public override async Task<CancelSwapResponse> CancelSwap(
        CancelSwapRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CancelSwapCommand
            {
                SwapRequestId = Guid.Parse(request.SwapRequestId),
                UserId = Guid.Parse(request.UserId)
            };

            await _mediator.Send(command);

            return new CancelSwapResponse
            {
                Success = true,
                Message = "Da huy yeu cau doi ca."
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// Nhan vien nha ca ve Cho Ca
    /// </summary>
    public override async Task<ReleaseShiftResponse> ReleaseShift(
        ReleaseShiftRequest request, ServerCallContext context)
    {
        try
        {
            var command = new ReleaseShiftCommand
            {
                AssignmentId = Guid.Parse(request.AssignmentId),
                UserId = Guid.Parse(request.UserId)
            };

            await _mediator.Send(command);

            return new ReleaseShiftResponse
            {
                Success = true,
                Message = "Da nha ca thanh cong. Ca da chuyen ve Cho Ca."
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }

    /// <summary>
    /// Xem danh sach yeu cau doi ca cua toi
    /// </summary>
    public override async Task<GetMySwapRequestsResponse> GetMySwapRequests(
        GetMySwapRequestsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new GetMySwapRequestsQuery
            {
                UserId = Guid.Parse(request.UserId)
            };

            var swaps = await _mediator.Send(query);

            var response = new GetMySwapRequestsResponse();
            foreach (var swap in swaps)
            {
                response.SwapRequests.Add(new SwapRequestItem
                {
                    Id = swap.Id.ToString(),
                    SourceUserId = swap.SourceUserId.ToString(),
                    SourceUserName = swap.SourceUserName ?? "",
                    TargetUserId = swap.TargetUserId.ToString(),
                    TargetUserName = swap.TargetUserName ?? "",
                    Status = swap.Status,
                    Reason = swap.Reason,
                    RejectReason = swap.RejectReason ?? "",
                    ExpiresAt = swap.ExpiresAt.ToString("o"),
                    CreatedAt = swap.CreatedAt.ToString("o"),
                    ShiftId = swap.ShiftId.ToString(),
                    ShiftDate = swap.ShiftDate.ToString("yyyy-MM-dd"),
                    StartTime = swap.StartTime.ToString("HH:mm"),
                    EndTime = swap.EndTime.ToString("HH:mm")
                });
            }

            return response;
        }
        catch (FormatException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Dinh dang du lieu khong hop le: {ex.Message}"));
        }
    }
}
