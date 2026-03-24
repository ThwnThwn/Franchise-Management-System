using Grpc.Core;
using MediatR;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CreateWarehouseTransfer;
using WarehouseService.Application.Features.WarehouseTransfers.Command.ApproveWarehouseTransfer;
using WarehouseService.Application.Features.WarehouseTransfers.Command.RejectWarehouseTransfer;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CompleteWarehouseTransfer;
using WarehouseService.Application.Features.WarehouseTransfers.Command.CancelWarehouseTransfer;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetById;
using WarehouseService.Application.Features.WarehouseTransfers.Query.GetAll;
using WarehouseService.Grpc.Protos.WarehouseTransfer;
using DomainTransferStatus = WarehouseService.Domain.Enums.TransferStatus;

namespace WarehouseService.API.Services;

public class WarehouseTransferGrpcService : WarehouseTransferGrpc.WarehouseTransferGrpcBase
{
    private readonly IMediator _mediator;

    public WarehouseTransferGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<CreateWarehouseTransferReply> CreateWarehouseTransfer(
        CreateWarehouseTransferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.FromWarehouseId, out var fromWarehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid source warehouse ID."));
        if (!Guid.TryParse(request.ToWarehouseId, out var toWarehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid destination warehouse ID."));

        var items = request.Items.Select(i =>
        {
            if (!Guid.TryParse(i.WarehouseItemId, out var itemId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid Warehouse Item ID: {i.WarehouseItemId}."));
            return new TransferItemDto(itemId, (decimal)i.Quantity);
        }).ToList();

        var result = await _mediator.Send(new CreateWarehouseTransferCommand(
            fromWarehouseId, toWarehouseId, items,
            request.HasReason ? request.Reason : null,
            request.HasNote ? request.Note : null,
            GetUserId(context)
        ), context.CancellationToken);

        var reply = new CreateWarehouseTransferReply
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            FromWarehouseId = result.FromWarehouseId.ToString(),
            ToWarehouseId = result.ToWarehouseId.ToString(),
            Status = ToProtoStatus(result.Status),
            Reason = result.Reason ?? string.Empty,
            Note = result.Note ?? string.Empty,
            CreatedBy = result.CreatedBy.ToString(),
            CreatedAt = result.CreatedAt.ToString("o"),
            Message = "Warehouse transfer created successfully."
        };

        reply.Items.AddRange(result.Items.Select(i => new TransferItemInfo
        {
            WarehouseItemId = i.WarehouseItemId.ToString(),
            IngredientId = i.IngredientId.ToString(),
            Unit = i.Unit,
            Quantity = (double)i.Quantity,
            UnitPrice = (double)i.UnitPrice
        }));

        return reply;
    }

    public override async Task<ApproveWarehouseTransferReply> ApproveWarehouseTransfer(
        ApproveWarehouseTransferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Transfer ID."));

        var result = await _mediator.Send(
            new ApproveWarehouseTransferCommand(id, GetUserId(context)),
            context.CancellationToken);

        return new ApproveWarehouseTransferReply
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            Status = ToProtoStatus(result.Status),
            ApprovedBy = result.ApprovedBy.ToString(),
            ApprovedAt = result.ApprovedAt.ToString("o"),
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse transfer approved successfully."
        };
    }

    public override async Task<RejectWarehouseTransferReply> RejectWarehouseTransfer(
        RejectWarehouseTransferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Transfer ID."));

        var result = await _mediator.Send(
            new RejectWarehouseTransferCommand(
                id,
                request.HasReason ? request.Reason : null,
                GetUserId(context)
            ), context.CancellationToken);

        return new RejectWarehouseTransferReply
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            Status = ToProtoStatus(result.Status),
            Reason = result.Reason ?? string.Empty,
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse transfer rejected successfully."
        };
    }

    public override async Task<CompleteWarehouseTransferReply> CompleteWarehouseTransfer(
        CompleteWarehouseTransferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Transfer ID."));

        var result = await _mediator.Send(
            new CompleteWarehouseTransferCommand(id, GetUserId(context)),
            context.CancellationToken);

        var reply = new CompleteWarehouseTransferReply
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            Status = ToProtoStatus(result.Status),
            FromWarehouseId = result.FromWarehouseId.ToString(),
            ToWarehouseId = result.ToWarehouseId.ToString(),
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse transfer completed successfully."
        };

        reply.TransferredItems.AddRange(result.TransferredItems.Select(t => new TransferredItemInfo
        {
            IngredientId = t.IngredientId.ToString(),
            Unit = t.Unit,
            Quantity = (double)t.Quantity,
            FromWarehouseItemId = t.FromWarehouseItemId.ToString(),
            ToWarehouseItemId = t.ToWarehouseItemId.ToString()
        }));

        return reply;
    }

    public override async Task<CancelWarehouseTransferReply> CancelWarehouseTransfer(
        CancelWarehouseTransferRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Transfer ID."));

        var result = await _mediator.Send(
            new CancelWarehouseTransferCommand(
                id,
                request.HasReason ? request.Reason : null,
                GetUserId(context)
            ), context.CancellationToken);

        return new CancelWarehouseTransferReply
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            Status = ToProtoStatus(result.Status),
            Reason = result.Reason ?? string.Empty,
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Warehouse transfer cancelled successfully."
        };
    }

    public override async Task<WarehouseTransferDetail> GetWarehouseTransferById(
        GetWarehouseTransferByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse Transfer ID."));

        var result = await _mediator.Send(new GetWarehouseTransferQuery(id), context.CancellationToken);

        var detail = new WarehouseTransferDetail
        {
            Id = result.Id.ToString(),
            TransferCode = result.TransferCode,
            FromWarehouseId = result.FromWarehouseId.ToString(),
            ToWarehouseId = result.ToWarehouseId.ToString(),
            Status = ToProtoStatus(result.Status),
            Reason = result.Reason ?? string.Empty,
            Note = result.Note ?? string.Empty,
            CreatedBy = result.CreatedBy.ToString(),
            ApprovedBy = result.ApprovedBy?.ToString() ?? string.Empty,
            ApprovedAt = result.ApprovedAt?.ToString("o") ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("o"),
            UpdatedAt = result.UpdatedAt.ToString("o")
        };

        // Include warehouse items from source warehouse
        if (result.Items is not null)
        {
            detail.Items.AddRange(result.Items.Select(i => new TransferItemInfo
            {
                WarehouseItemId = i.WarehouseItemId.ToString(),
                IngredientId = i.IngredientId.ToString(),
                Unit = i.Unit,
                Quantity = (double)i.Quantity,
                UnitPrice = (double)i.UnitPrice
            }));
        }

        return detail;
    }

    public override async Task<GetWarehouseTransfersReply> GetWarehouseTransfers(
        GetWarehouseTransfersRequest request, ServerCallContext context)
    {
        DomainTransferStatus? status = null;
        if (request.HasStatus)
            status = ToDomainStatus(request.Status);

        var result = await _mediator.Send(new GetWarehouseTransfersQuery(
            ParseOptionalGuid(request.FromWarehouseId, "source warehouse ID"),
            ParseOptionalGuid(request.ToWarehouseId, "destination warehouse ID"),
            status,
            string.IsNullOrWhiteSpace(request.FromDate) ? null : DateTimeOffset.Parse(request.FromDate).ToUniversalTime(),
            string.IsNullOrWhiteSpace(request.ToDate) ? null : DateTimeOffset.Parse(request.ToDate).ToUniversalTime(),
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10
        ), context.CancellationToken);

        var reply = new GetWarehouseTransfersReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        reply.Items.AddRange(result.Items.Select(t => new WarehouseTransferSummary
        {
            Id = t.Id.ToString(),
            TransferCode = t.TransferCode,
            FromWarehouseId = t.FromWarehouseId.ToString(),
            ToWarehouseId = t.ToWarehouseId.ToString(),
            Status = ToProtoStatus(t.Status),
            Reason = t.Reason ?? string.Empty,
            CreatedBy = t.CreatedBy.ToString(),
            ApprovedBy = t.ApprovedBy?.ToString() ?? string.Empty,
            ApprovedAt = t.ApprovedAt?.ToString("o") ?? string.Empty,
            CreatedAt = t.CreatedAt.ToString("o")
        }));

        return reply;
    }

    

    private static TransferStatus ToProtoStatus(DomainTransferStatus domain) =>
        domain switch
        {
            DomainTransferStatus.Draft => TransferStatus.Draft,
            DomainTransferStatus.Pending => TransferStatus.Pending,
            DomainTransferStatus.Approved => TransferStatus.Approved,
            DomainTransferStatus.InTransit => TransferStatus.InTransit,
            DomainTransferStatus.Completed => TransferStatus.Completed,
            DomainTransferStatus.Cancelled => TransferStatus.Cancelled,
            DomainTransferStatus.Rejected => TransferStatus.Rejected,
            _ => TransferStatus.Unspecified
        };

    private static DomainTransferStatus ToDomainStatus(TransferStatus proto) =>
        proto switch
        {
            TransferStatus.Draft => DomainTransferStatus.Draft,
            TransferStatus.Pending => DomainTransferStatus.Pending,
            TransferStatus.Approved => DomainTransferStatus.Approved,
            TransferStatus.InTransit => DomainTransferStatus.InTransit,
            TransferStatus.Completed => DomainTransferStatus.Completed,
            TransferStatus.Cancelled => DomainTransferStatus.Cancelled,
            TransferStatus.Rejected => DomainTransferStatus.Rejected,
            _ => DomainTransferStatus.Pending
        };

    // ── Other helpers ─────────────────────────────────────────

    private static Guid GetUserId(ServerCallContext context)
    {
        var userIdHeader = context.RequestHeaders.GetValue("x-user-id");
        if (string.IsNullOrWhiteSpace(userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing or invalid x-user-id header."));
        return userId;
    }

    private static Guid? ParseOptionalGuid(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!Guid.TryParse(value, out var guid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid {fieldName}."));
        return guid;
    }
}