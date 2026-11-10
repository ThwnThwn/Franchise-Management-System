using MediatR;
using SupplierService.Application.Repositories;
using SupplierService.Domain.Entities;
using SupplierService.Domain.Enums;

namespace SupplierService.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;

/// <summary>
/// Handler cập nhật đơn nhập hàng:
/// 1. Load PO kèm Items
/// 2. Validate: chỉ cho sửa khi Status = Pending
/// 3. Tra cứu unit_price từ supplier_ingredients
/// 4. Cập nhật fields, thay toàn bộ items
/// 5. Tính lại TotalAmount / NetAmount
/// 6. Save
/// </summary>
public class UpdatePurchaseOrderCommandHandler(IPurchaseOrderRepository purchaseOrderRepo
) : IRequestHandler<UpdatePurchaseOrderCommand>
{
    public async Task<Unit> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Load PO
        var po = await purchaseOrderRepo.GetByIdWithItemsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found.");

        // 2. Validate status
        if (po.Status != PurchaseOrderStatus.Pending)
            throw new InvalidOperationException($"Cannot update purchase order with status '{po.Status}'. Only 'Pending' orders can be updated.");

        // 3. Validate supplier
        var supplierExists = await purchaseOrderRepo.SupplierExistsAsync(request.SupplierId, cancellationToken);
        if (!supplierExists)
            throw new KeyNotFoundException($"Supplier {request.SupplierId} not found.");

        // 4. Validate items
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Purchase order must have at least one item.");

        // 5. Tra cứu unit_price từ supplier_ingredients
        var ingredientIds = request.Items.Select(i => i.IngredientId).ToList();
        var priceMap = await purchaseOrderRepo.GetSupplierIngredientPricesAsync(
            request.SupplierId, ingredientIds, cancellationToken);

        var missingIds = ingredientIds.Where(id => !priceMap.ContainsKey(id)).ToList();
        if (missingIds.Any())
            throw new KeyNotFoundException(
                $"Ingredients not found for this supplier: {string.Join(", ", missingIds)}");

        // 6. Update fields
        po.SupplierId = request.SupplierId;
        po.StoreId = request.StoreId;
        po.Notes = request.Notes;
        po.ReferenceCode = request.ReferenceCode;
        po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        po.DiscountAmount = request.DiscountAmount;

        // 7. Replace items
        po.Items.Clear();

        decimal totalAmount = 0;
        foreach (var itemDto in request.Items)
        {
            var unitPrice = priceMap[itemDto.IngredientId];
            var totalPrice = itemDto.Quantity * unitPrice;
            totalAmount += totalPrice;

            po.Items.Add(new PurchaseOrderItem
            {
                PurchaseOrderId = po.Id,
                IngredientId = itemDto.IngredientId,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            });
        }

        po.TotalAmount = totalAmount;
        po.NetAmount = totalAmount - request.DiscountAmount;

        // 8. Save
        await purchaseOrderRepo.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}