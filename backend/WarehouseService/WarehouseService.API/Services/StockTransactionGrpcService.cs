using Grpc.Core;
using MediatR;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockIn;
using WarehouseService.Application.Features.StockTransactions.Command.CreateStockOut;
using WarehouseService.Application.Features.StockTransactions.Command.CancelStockTransaction;
using WarehouseService.Application.Features.StockTransactions.Command.CompleteStockTransaction;
using WarehouseService.Application.Features.StockTransactions.Query.GetAll;
using WarehouseService.Application.Features.StockTransactions.Query.GetById;
using WarehouseService.Grpc.Protos.StockTransaction;
using DomainTransactionType = WarehouseService.Domain.Enums.TransactionType;
using DomainTransactionStatus = WarehouseService.Domain.Enums.TransactionStatus;

namespace WarehouseService.API.Services;

public class StockTransactionGrpcService : StockTransactionGrpc.StockTransactionGrpcBase
{
    private readonly IMediator _mediator;

    public StockTransactionGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<StockInReply> CreateStockIn(
        CreateStockInRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WarehouseId, out var warehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        Guid? supplierId = null;
        if (request.HasSupplierId)
        {
            if (!Guid.TryParse(request.SupplierId, out var parsedSupplierId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Supplier ID."));
            supplierId = parsedSupplierId;
        }

        var items = request.Items.Select(i =>
        {
            if (!Guid.TryParse(i.IngredientId, out var ingredientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid Ingredient ID: {i.IngredientId}."));
            return new StockInItemDto(ingredientId, (decimal)i.Quantity, (decimal)i.UnitPrice);
        }).ToList();

        var result = await _mediator.Send(new CreateStockInCommand(
            warehouseId,
            items,
            supplierId,
            request.HasReferenceCode ? request.ReferenceCode : null,
            request.HasNote ? request.Note : null,
            GetUserId(context)
        ), context.CancellationToken);

        var reply = new StockInReply
        {
            WarehouseId = result.WarehouseId.ToString(),
            SupplierId = result.SupplierId?.ToString() ?? string.Empty,
            ReferenceCode = result.ReferenceCode ?? string.Empty,
            TotalItems = result.TotalItems,
            TotalAmount = (double)result.TotalAmount,
            CreatedAt = result.CreatedAt.ToString("o"),
            Message = "Stock in created successfully."
        };

        reply.Transactions.AddRange(result.Transactions.Select(t => new StockTransactionItem
        {
            Id = t.Id.ToString(),
            TransactionCode = t.TransactionCode,
            WarehouseItemId = t.WarehouseItemId.ToString(),
            IngredientId = t.IngredientId.ToString(),
            Type = ToProtoType(t.Type),
            Quantity = (double)t.Quantity,
            UnitPrice = (double)t.UnitPrice,
            TotalPrice = (double)t.TotalPrice,
            Status = ToProtoStatus(t.Status)
        }));

        return reply;
    }

    public override async Task<StockOutReply> CreateStockOut(
        CreateStockOutRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.WarehouseId, out var warehouseId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Warehouse ID."));

        var items = request.Items.Select(i =>
        {
            if (!Guid.TryParse(i.IngredientId, out var ingredientId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid Ingredient ID: {i.IngredientId}."));
            return new StockOutItemDto(ingredientId, (decimal)i.Quantity, (decimal)i.UnitPrice);
        }).ToList();

        var result = await _mediator.Send(new CreateStockOutCommand(
            warehouseId,
            items,
            request.HasNote ? request.Note : null,
            GetUserId(context)
        ), context.CancellationToken);

        var reply = new StockOutReply
        {
            WarehouseId = result.WarehouseId.ToString(),
            TotalItems = result.TotalItems,
            TotalAmount = (double)result.TotalAmount,
            CreatedAt = result.CreatedAt.ToString("o"),
            Message = "Stock out created successfully."
        };

        reply.Transactions.AddRange(result.Transactions.Select(t => new StockTransactionItem
        {
            Id = t.Id.ToString(),
            TransactionCode = t.TransactionCode,
            WarehouseItemId = t.WarehouseItemId.ToString(),
            IngredientId = t.IngredientId.ToString(),
            Type = ToProtoType(t.Type),
            Quantity = (double)t.Quantity,
            UnitPrice = (double)t.UnitPrice,
            TotalPrice = (double)t.TotalPrice,
            Status = ToProtoStatus(t.Status)
        }));

        return reply;
    }

    public override async Task<CompleteStockTransactionReply> CompleteStockTransaction(
        CompleteStockTransactionRequest request, ServerCallContext context)
    {
        var transactionIds = request.TransactionIds.Select(id =>
        {
            if (!Guid.TryParse(id, out var guid))
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid Transaction ID: {id}."));
            return guid;
        }).ToList();

        var result = await _mediator.Send(
            new CompleteStockTransactionCommand(transactionIds, GetUserId(context)),
            context.CancellationToken);

        var reply = new CompleteStockTransactionReply
        {
            TotalCompleted = result.TotalCompleted,
            TotalAmount = (double)result.TotalAmount,
            CompletedAt = result.CompletedAt.ToString("o"),
            Message = "Stock transactions completed successfully."
        };

        reply.Transactions.AddRange(result.Transactions.Select(t => new CompletedTransactionItem
        {
            Id = t.Id.ToString(),
            TransactionCode = t.TransactionCode,
            WarehouseItemId = t.WarehouseItemId.ToString(),
            Type = ToProtoType(t.Type),
            Quantity = (double)t.Quantity,
            TotalPrice = (double)t.TotalPrice,
            Status = ToProtoStatus(t.Status)
        }));

        return reply;
    }

    public override async Task<CancelStockTransactionReply> CancelStockTransaction(
        CancelStockTransactionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Stock Transaction ID."));

        var result = await _mediator.Send(
            new CancelStockTransactionCommand(id, GetUserId(context)),
            context.CancellationToken);

        return new CancelStockTransactionReply
        {
            Id = result.Id.ToString(),
            TransactionCode = result.TransactionCode,
            WarehouseId = result.WarehouseId.ToString(),
            WarehouseItemId = result.WarehouseItemId.ToString(),
            Type = ToProtoType(result.Type),
            Status = ToProtoStatus(result.Status),
            UpdatedAt = result.UpdatedAt.ToString("o"),
            Message = "Stock transaction cancelled successfully."
        };
    }

    public override async Task<StockTransactionDetail> GetStockTransactionById(
        GetStockTransactionByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Stock Transaction ID."));

        var result = await _mediator.Send(new GetStockTransactionQuery(id), context.CancellationToken);

        return new StockTransactionDetail
        {
            Id = result.Id.ToString(),
            TransactionCode = result.TransactionCode,
            WarehouseId = result.WarehouseId.ToString(),
            WarehouseItemId = result.WarehouseItemId.ToString(),
            IngredientId = result.IngredientId.ToString() ?? string.Empty,
            Type = ToProtoType(result.Type),
            Quantity = (double)result.Quantity,
            UnitPrice = (double)result.UnitPrice,
            TotalPrice = (double)result.TotalPrice,
            SupplierId = result.SupplierId?.ToString() ?? string.Empty,
            ReferenceCode = result.ReferenceCode ?? string.Empty,
            WarehouseTransferId = result.WarehouseTransferId?.ToString() ?? string.Empty,
            Note = result.Note ?? string.Empty,
            Status = ToProtoStatus(result.Status),
            CreatedBy = result.CreatedBy.ToString(),
            CreatedAt = result.CreatedAt.ToString("o"),
            UpdatedAt = result.UpdatedAt.ToString("o")
        };
    }

    public override async Task<GetStockTransactionsReply> GetStockTransactions(
        GetStockTransactionsRequest request, ServerCallContext context)
    {
        DomainTransactionType? type = null;
        if (request.HasType)
            type = ToDomainType(request.Type);

        DomainTransactionStatus? status = null;
        if (request.HasStatus)
            status = ToDomainStatus(request.Status);

        var result = await _mediator.Send(new GetStockTransactionsQuery(
            request.HasWarehouseId ? ParseOptionalGuid(request.WarehouseId, "Warehouse ID") : null,
            request.HasWarehouseItemId ? ParseOptionalGuid(request.WarehouseItemId, "Warehouse Item ID") : null,
            type,
            status,
            request.HasSupplierId ? ParseOptionalGuid(request.SupplierId, "Supplier ID") : null,
            request.HasReferenceCode ? request.ReferenceCode : null,
            request.HasFromDate ? DateTimeOffset.Parse(request.FromDate!).ToUniversalTime() : null,
            request.HasToDate ? DateTimeOffset.Parse(request.ToDate!).ToUniversalTime() : null,
            request.Page > 0 ? request.Page : 1,
            request.PageSize > 0 ? request.PageSize : 10
        ), context.CancellationToken);

        var reply = new GetStockTransactionsReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        reply.Items.AddRange(result.Items.Select(t => new StockTransactionSummary
        {
            Id = t.Id.ToString(),
            TransactionCode = t.TransactionCode,
            WarehouseId = t.WarehouseId.ToString(),
            WarehouseItemId = t.WarehouseItemId.ToString(),
            IngredientId = t.IngredientId.ToString(),
            Type = ToProtoType(t.Type),
            Quantity = (double)t.Quantity,
            UnitPrice = (double)t.UnitPrice,
            TotalPrice = (double)t.TotalPrice,
            SupplierId = t.SupplierId?.ToString() ?? string.Empty,
            ReferenceCode = t.ReferenceCode ?? string.Empty,
            TransferId = t.WarehouseTransferId?.ToString() ?? string.Empty,
            Status = ToProtoStatus(t.Status),
            CreatedBy = t.CreatedBy.ToString(),
            CreatedAt = t.CreatedAt.ToString("o")
        }));

        return reply;
    }

    // ── Enum mapping helpers ──────────────────────────────────
    // Domain starts at 1, Proto starts at 0 → offset = -1

    private static TransactionType ToProtoType(DomainTransactionType domain) =>
        domain switch
        {
            DomainTransactionType.StockIn => TransactionType.StockIn,
            DomainTransactionType.StockOut => TransactionType.StockOut,
            DomainTransactionType.TransferIn => TransactionType.TransferIn,
            DomainTransactionType.TransferOut => TransactionType.TransferOut,
            DomainTransactionType.Adjustment => TransactionType.Adjustment,
            DomainTransactionType.Return => TransactionType.Return,
            _ => TransactionType.StockIn
        };

    private static DomainTransactionType ToDomainType(TransactionType proto) =>
        proto switch
        {
            TransactionType.StockIn => DomainTransactionType.StockIn,
            TransactionType.StockOut => DomainTransactionType.StockOut,
            TransactionType.TransferIn => DomainTransactionType.TransferIn,
            TransactionType.TransferOut => DomainTransactionType.TransferOut,
            TransactionType.Adjustment => DomainTransactionType.Adjustment,
            TransactionType.Return => DomainTransactionType.Return,
            _ => DomainTransactionType.StockIn
        };

    private static TransactionStatus ToProtoStatus(DomainTransactionStatus domain) =>
        domain switch
        {
            DomainTransactionStatus.Pending => TransactionStatus.Pending,
            DomainTransactionStatus.Completed => TransactionStatus.Completed,
            DomainTransactionStatus.Cancelled => TransactionStatus.Cancelled,
            _ => TransactionStatus.Pending
        };

    private static DomainTransactionStatus ToDomainStatus(TransactionStatus proto) =>
        proto switch
        {
            TransactionStatus.Pending => DomainTransactionStatus.Pending,
            TransactionStatus.Completed => DomainTransactionStatus.Completed,
            TransactionStatus.Cancelled => DomainTransactionStatus.Cancelled,
            _ => DomainTransactionStatus.Pending
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