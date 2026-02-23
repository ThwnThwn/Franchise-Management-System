import { OrderStatus, PaymentMethod } from "./enum"

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
    [PaymentMethod.Unknown]: "Chưa chọn",
    [PaymentMethod.Cash]: "Tiền mặt",
    [PaymentMethod.VNPay]: "VNPay"
}

// export const OrderStatusLabels: Record<OrderStatus, string> = {
//     [OrderStatus.PaymentPending]: "Chưa thanh toán",
//     [OrderStatus.Preparing]: "Đang chuẩn bị",
//     [OrderStatus.Ready]: "Đang giao",
//     [OrderStatus.Completed]: "Hoàn thành",
//     [OrderStatus.Cancel]: "Huỷ"
// }

// export const OrderStatusNote: Record<OrderStatus, string> = {
//     [OrderStatus.PaymentPending]: "Vui lòng hoàn tất thanh toán để nhà hàng tiếp nhận đơn.",
//     [OrderStatus.Preparing]: "Bếp đang nổi lửa chế biến món ngon của bạn, vui lòng đợi giây lát.",
//     [OrderStatus.Ready]: "Món đã xong! Bạn có thể nhận tại quầy hoặc đợi tài xế đến lấy.",
//     [OrderStatus.Completed]: "Giao dịch hoàn tất. Chúc bạn ngon miệng và hẹn gặp lại!",
//     [OrderStatus.Cancel]: "Đơn hàng này không còn hiệu lực do đã bị huỷ."
// }

export const OrderStatusLabels: Record<OrderStatus, string> = {
    [OrderStatus.PaymentPending]: "Chờ thanh toán",
    [OrderStatus.ConfirmationPending]: "Chờ xác nhận",
    [OrderStatus.Confirmed]: "Đã xác nhận",
    [OrderStatus.Preparing]: "Đang chuẩn bị",
    [OrderStatus.ReadyForPickup]: "Sẵn sàng giao",
    [OrderStatus.Delivering]: "Đang giao",
    [OrderStatus.Completed]: "Hoàn thành",
    [OrderStatus.Cancelled]: "Huỷ"
}

export const OrderStatusNote: Record<OrderStatus, string> = {
    [OrderStatus.PaymentPending]: "Vui lòng hoàn tất thanh toán để nhà hàng tiếp nhận đơn.",
    [OrderStatus.ConfirmationPending]: "Đã nhận thanh toán, đang chờ nhà hàng xác nhận đơn hàng.",
    [OrderStatus.Confirmed]: "Đơn hàng đã được xác nhận và chuẩn bị chuyển cho bộ phận bếp.",
    [OrderStatus.Preparing]: "Bếp đang nổi lửa chế biến món ngon của bạn, vui lòng đợi giây lát.",
    [OrderStatus.ReadyForPickup]: "Món đã xong! Bạn có thể nhận tại quầy hoặc đợi tài xế đến lấy.",
    [OrderStatus.Delivering]: "Đơn hàng đang trên đường giao đến bạn.",
    [OrderStatus.Completed]: "Giao dịch hoàn tất. Chúc bạn ngon miệng và hẹn gặp lại!",
    [OrderStatus.Cancelled]: "Đơn hàng này không còn hiệu lực do đã bị huỷ."
}
