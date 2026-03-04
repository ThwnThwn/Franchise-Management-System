using MediatR;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

/// <summary>
/// Handler tạo mới đơn nhập hàng:
/// 1. Validate supplier tồn tại
/// 2. Validate items không rỗng
/// 3. Tra cứu unit_price từ supplier_ingredients
/// 4. Generate mã PO tự động (PO-YYYYMMDD-NNN)
/// 5. Tạo PO entity + Items, tính TotalAmount / NetAmount
/// 6. Save → trả về Id + Code
/// </summary>
public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, CreatePurchaseOrderResult>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepo;
    private readonly SupplierService.Application.ClientInterfaces.IFranchiseServiceClient _franchiseServiceClient;

    public CreatePurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepo,
        SupplierService.Application.ClientInterfaces.IFranchiseServiceClient franchiseServiceClient)
    {
        _purchaseOrderRepo = purchaseOrderRepo;
        _franchiseServiceClient = franchiseServiceClient;
    }

    public async Task<CreatePurchaseOrderResult> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate supplier
        var supplierExists = await _purchaseOrderRepo.SupplierExistsAsync(request.SupplierId, cancellationToken);
        if (!supplierExists)
            throw new KeyNotFoundException($"Supplier {request.SupplierId} not found.");

        // 2. Validate items
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Purchase order must have at least one item.");

        if (request.StoreId == null)
            throw new InvalidOperationException("StoreId is required.");

        var storeExists = await _franchiseServiceClient.GetFranchiseByIdAsync(request.StoreId.Value);
        if (!storeExists)
            throw new KeyNotFoundException($"Store {request.StoreId} does not exist.");

        // 3. Tra cứu unit_price từ supplier_ingredients
        var ingredientIds = request.Items.Select(i => i.IngredientId).ToList();
        var priceMap = await _purchaseOrderRepo.GetSupplierIngredientPricesAsync(
            request.SupplierId, ingredientIds, cancellationToken);

        // Kiểm tra tất cả ingredients đều có trong bảng giá của supplier
        var missingIds = ingredientIds.Where(id => !priceMap.ContainsKey(id)).ToList();
        if (missingIds.Any())
            throw new KeyNotFoundException(
                $"Ingredients not found for this supplier: {string.Join(", ", missingIds)}");

        // 4. Generate code
        var code = await _purchaseOrderRepo.GenerateNextCodeAsync(cancellationToken);

        // 5. Build entity
        var po = new PurchaseOrder
        {
            Code = code,
            SupplierId = request.SupplierId,
            StoreId = request.StoreId,
            Status = PurchaseOrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            OrderDate = DateTimeOffset.UtcNow,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes,
            ReferenceCode = request.ReferenceCode,
            DiscountAmount = request.DiscountAmount
        };

        decimal totalAmount = 0;
        foreach (var itemDto in request.Items)
        {
            var unitPrice = priceMap[itemDto.IngredientId];
            var totalPrice = itemDto.Quantity * unitPrice;
            totalAmount += totalPrice;

            po.Items.Add(new PurchaseOrderItem
            {
                IngredientId = itemDto.IngredientId,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            });
        }

        po.TotalAmount = totalAmount;
        po.NetAmount = totalAmount - request.DiscountAmount;

        // 6. Save
        await _purchaseOrderRepo.AddAsync(po, cancellationToken);
        await _purchaseOrderRepo.SaveChangesAsync(cancellationToken);

        return new CreatePurchaseOrderResult(po.Id, po.Code);
    }
}

