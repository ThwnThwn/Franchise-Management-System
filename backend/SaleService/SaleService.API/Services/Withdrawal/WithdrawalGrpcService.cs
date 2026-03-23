using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using SalesService.Application.Features.WithdrawalRequests.Commands.ApproveWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CancelMyWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CancelWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CompleteWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.CreateWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Commands.RejectWithdrawal;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetAllWithdrawals;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetMyWithdrawals;
using SalesService.Application.Features.WithdrawalRequests.Queries.GetWithdrawalById;

namespace SalesService.API.Services.Withdrawal;

public class WithdrawalGrpcService(IMediator mediator) : WithdrawalService.WithdrawalServiceBase
{
    private readonly IMediator _mediator = mediator;

    // ==============================================================================
    // NHÓM END-USER (KHÁCH HÀNG)
    // ==============================================================================

    public override async Task<WithdrawalBaseResponse> CreateWithdrawal(CreateWithdrawalRequest request, ServerCallContext context)
    {
        var command = new CreateWithdrawalCommand(
            GetUserId(context),
            request.Amount,
            request.BankCode,
            request.BankName,
            request.AccountName,
            request.AccountNumber,
            request.Notes
        );

        var id = await _mediator.Send(command);

        return new WithdrawalBaseResponse
        {
            Id = id.ToString(),
            Message = "Tạo yêu cầu rút tiền thành công. Vui lòng chờ hệ thống xử lý."
        };
    }

    public override async Task<WithdrawalListResponse> GetMyWithdrawals(GetMyWithdrawalsRequest request, ServerCallContext context)
    {
        Domain.Enums.WithdrawalStatus? status = (Domain.Enums.WithdrawalStatus)request.Status;
        if ((int)status <= 0)
        {
            status = null;
        }

        var query = new GetMyWithdrawalsQuery(
            GetUserId(context),
            status,
            request.PageIndex,
            request.PageSize
        );

        var result = await _mediator.Send(query);
        return MapToPagedResponse(result);
    }

    public override async Task<Empty> CancelMyWithdrawal(CancelMyWithdrawalRequest request, ServerCallContext context)
    {
        var command = new CancelMyWithdrawalCommand(
            Guid.Parse(request.RequestId),
            GetUserId(context),
            request.CancellationReason
        );

        await _mediator.Send(command);
        return new Empty();
    }

    // ==============================================================================
    // NHÓM ADMIN (QUẢN TRỊ VIÊN / KẾ TOÁN)
    // ==============================================================================

    public override async Task<WithdrawalListResponse> GetAllWithdrawals(GetAllWithdrawalsRequest request, ServerCallContext context)
    {
        Domain.Enums.WithdrawalStatus? status = (Domain.Enums.WithdrawalStatus)request.Status;
        if ((int)status <= 0)
        {
            status = null;
        }

        var query = new GetAllWithdrawalsQuery(
            string.IsNullOrEmpty(request.UserId) ? null : Guid.Parse(request.UserId),
            status,
            request.PageIndex,
            request.PageSize
        );

        var result = await _mediator.Send(query);
        return MapToPagedResponse(result);
    }

    public override async Task<WithdrawalDetailResponse> GetWithdrawalById(GetWithdrawalByIdRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetWithdrawalByIdQuery(Guid.Parse(request.RequestId)))
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Không tìm thấy thông tin yêu cầu rút tiền này."));

        return new WithdrawalDetailResponse
        {
            Id = result.Id.ToString(),
            RequestCode = result.RequestCode,
            UserId = result.UserId.ToString(),
            Amount = result.Amount ?? "0.00",
            FeeAmount = result.FeeAmount ?? "0.00",
            BankCode = result.BankCode ?? "",
            BankName = result.BankName,
            AccountName = result.AccountName,
            MaskedAccountNumber = result.MaskedAccountNumber,
            RawAccountNumber = result.RawAccountNumber, // Đã giải mã AES ở Handler
            Status = (WithdrawalStatus)result.Status,
            Notes = result.Notes ?? "",
            InternalNotes = result.InternalNotes ?? "",
            ApprovalNotes = result.ApprovalNotes ?? "",
            RejectionReason = result.RejectionReason ?? "",
            CancellationReason = result.CancellationReason ?? "",
            ReceiptImageUrl = result.ReceiptImageUrl ?? "",
            TransactionId = result.TransactionId?.ToString() ?? "",
            CreatedAt = result.CreatedAt.ToString("O"), // ISO 8601
            UpdatedAt = result.UpdatedAt.ToString("O"),

            BankTransactionId = result.BankTransactionId ?? "",
            CompletedAt = result.CompletedAt?.ToString("O") ?? "",
            ProcessedById = result.ProcessedById?.ToString() ?? "",
            ProcessedByName = result.ProcessedByName ?? ""
        };
    }

    public override async Task<Empty> ApproveWithdrawal(ApproveWithdrawalRequest request, ServerCallContext context)
    {
        await _mediator.Send(new ApproveWithdrawalCommand(
            RequestId: Guid.Parse(request.RequestId),
            AdminId: GetUserId(context),
            ApprovalNotes: request.ApprovalNotes));
        return new Empty();
    }

    public override async Task<Empty> RejectWithdrawal(RejectWithdrawalRequest request, ServerCallContext context)
    {
        await _mediator.Send(new RejectWithdrawalCommand(
            RequestId: Guid.Parse(request.RequestId),
            AdminId: GetUserId(context),
            RejectionReason: request.RejectionReason));
        return new Empty();
    }

    public override async Task<Empty> CancelWithdrawal(CancelWithdrawalRequest request, ServerCallContext context)
    {
        await _mediator.Send(new CancelWithdrawalCommand(
            RequestId: Guid.Parse(request.RequestId),
            AdminId: GetUserId(context),
            Reason: request.CancellationReason));
        return new Empty();
    }

    public override async Task<Empty> CompleteWithdrawal(CompleteWithdrawalRequest request, ServerCallContext context)
    {
        // Gateway gởi bytes qua FormFile -> ByteString -> byte[]
        var command = new CompleteWithdrawalCommand(
            RequestId: Guid.Parse(request.RequestId),
            AdminId: GetUserId(context),
            ReceiptFile: request.ReceiptFile.ToByteArray(),
            BankTransactionId: request.BankTransactionId
        );

        await _mediator.Send(command);
        return new Empty();
    }

    // ==============================================================================
    // HELPERS & MAPPING
    // ==============================================================================

    private static Guid GetUserId(ServerCallContext context)
    {
        var userIdStr = context.RequestHeaders.GetValue("x-user-id");

        if (string.IsNullOrEmpty(userIdStr))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Không tìm thấy định danh người dùng (x-user-id)."));

        return Guid.Parse(userIdStr);
    }

    private static WithdrawalListResponse MapToPagedResponse(BuildingBlocks.Core.Pagination.PagedResult<Application.Dtos.WithdrawalRequests.WithdrawalSummaryDto> result)
    {
        var response = new WithdrawalListResponse
        {
            TotalCount = (int)result.TotalCount,
            TotalPages = result.TotalPages
        };

        response.Items.AddRange(result.Items.Select(x => new WithdrawalSummary
        {
            Id = x.Id.ToString(),
            RequestCode = x.RequestCode,
            UserId = x.UserId.ToString(),
            Amount = x.Amount ?? "0.00",
            FeeAmount = x.FeeAmount ?? "0.00",
            BankCode = x.BankCode ?? "",
            BankName = x.BankName,
            AccountName = x.AccountName,
            MaskedAccountNumber = x.MaskedAccountNumber,
            Status = (WithdrawalStatus)x.Status,
            CreatedAt = x.CreatedAt.ToString("O")
        }));

        return response;
    }
}