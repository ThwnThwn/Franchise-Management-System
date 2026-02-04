using MediatR;
using SharedKernel.Events;
using System;
using System.Collections.Generic;

namespace InventoryService.Application.Features.StoreIngredient.Commands.ConfirmStockReservation
{
    /// <summary>
    /// Command xác nhận giữ hàng (Confirm Stock Reservation).
    /// Được gọi khi đơn hàng thanh toán thành công.
    /// Logic: Xóa hàng giữ trong Queue -> Trừ kho thật.
    /// </summary>
    public record ConfirmStockReservationCommand(Guid OrderId, Guid StoreId, List<OrderPaidItem> Items) : IRequest<bool>;
}
