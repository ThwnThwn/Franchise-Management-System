using Grpc.Core;
using MediatR;
using SupplierService.Application.Features.Suppliers.Commands.CreateSupplier;
using SupplierService.Application.Features.Suppliers.Commands.DeleteSupplier;
using SupplierService.Application.Features.Suppliers.Commands.UpdateSupplier;
using SupplierService.Application.Features.Suppliers.Queries.GetSupplierById;
using SupplierService.Application.Features.Suppliers.Queries.GetSuppliers;
using SupplierService.Grpc.Protos.Supplier;

namespace SupplierService.API.Services.Suppliers;

/// <summary>
/// gRPC service quản lý nhà cung cấp (Supplier).
/// Cung cấp CRUD: Create, Update, Delete (soft), GetById, GetAll (phân trang).
/// </summary>
public class SupplierGrpcService : SupplierGrpc.SupplierGrpcBase
{
    private readonly IMediator _mediator;

    public SupplierGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo mới nhà cung cấp.
    /// Kiểm tra trùng Code. Trả về ID + Code sau khi tạo thành công.
    /// </summary>
    public override async Task<CreateSupplierReply> CreateSupplier(
        CreateSupplierRequest request, ServerCallContext context)
    {
        var command = new CreateSupplierCommand
        {
            Name = request.Name,
            ContactPerson = request.HasContactPerson ? request.ContactPerson : null,
            Phone = request.HasPhone ? request.Phone : null,
            Email = request.HasEmail ? request.Email : null,
            Address = request.HasAddress ? request.Address : null,
            TaxCode = request.HasTaxCode ? request.TaxCode : null
        };

        var result = await _mediator.Send(command, context.CancellationToken);

        return new CreateSupplierReply
        {
            Id = result.Id.ToString(),
            Code = result.Code,
            Message = "Supplier created successfully."
        };
    }

    /// <summary>
    /// Cập nhật thông tin nhà cung cấp.
    /// Kiểm tra tồn tại + trùng Code trước khi update.
    /// </summary>
    public override async Task<UpdateSupplierReply> UpdateSupplier(
        UpdateSupplierRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Supplier ID."));

        var command = new UpdateSupplierCommand
        {
            Id = id,
            Name = request.HasName ? request.Name : null,
            ContactPerson = request.HasContactPerson ? request.ContactPerson : null,
            Phone = request.HasPhone ? request.Phone : null,
            Email = request.HasEmail ? request.Email : null,
            Address = request.HasAddress ? request.Address : null,
            TaxCode = request.HasTaxCode ? request.TaxCode : null,
            IsActive = request.HasIsActive ? request.IsActive : null
        };

        await _mediator.Send(command, context.CancellationToken);

        return new UpdateSupplierReply
        {
            Message = "Supplier updated successfully."
        };
    }

    /// <summary>
    /// Xóa mềm nhà cung cấp (set IsDeleted = true).
    /// Không xóa vật lý, giữ lịch sử PO liên quan.
    /// </summary>
    public override async Task<DeleteSupplierReply> DeleteSupplier(
        DeleteSupplierRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Supplier ID."));

        await _mediator.Send(new DeleteSupplierCommand(id), context.CancellationToken);

        return new DeleteSupplierReply
        {
            Message = "Supplier deleted successfully."
        };
    }

    /// <summary>
    /// Lấy chi tiết 1 nhà cung cấp theo ID.
    /// Trả về toàn bộ thông tin: code, tên, liên hệ, địa chỉ, trạng thái, timestamps.
    /// </summary>
    public override async Task<SupplierDetail> GetSupplierById(
        GetSupplierByIdRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetSupplierByIdQuery(request.Id), context.CancellationToken);

        return new SupplierDetail
        {
            Id = result.Id,
            Code = result.Code,
            Name = result.Name,
            ContactPerson = result.ContactPerson,
            Phone = result.Phone,
            Email = result.Email,
            Address = result.Address,
            TaxCode = result.TaxCode,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt
        };
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp có phân trang.
    /// Hỗ trợ tìm kiếm theo tên/code (search) và lọc theo trạng thái (is_active).
    /// </summary>
    public override async Task<GetSuppliersReply> GetSuppliers(
        GetSuppliersRequest request, ServerCallContext context)
    {
        var query = new GetSuppliersQuery
        {
            Code = request.HasCode ? request.Code : null,
            Name = request.HasName ? request.Name : null,
            IsActive = request.HasIsActive ? request.IsActive : null,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query, context.CancellationToken);

        var reply = new GetSuppliersReply
        {
            TotalCount = result.TotalCount
        };

        reply.Suppliers.AddRange(result.Suppliers.Select(s => new Grpc.Protos.Supplier.SupplierSummary
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Phone = s.Phone,
            IsActive = s.IsActive
        }));

        return reply;
    }
}
