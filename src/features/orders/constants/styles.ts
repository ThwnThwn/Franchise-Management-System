import { ChipProps } from "@heroui/react"
import { OrderStatus } from "./enum"

// export const OrderStatusColors: Record<OrderStatus, ChipProps["color"]> = {
//     [OrderStatus.PaymentPending]: "default", // Xám: Chưa có dòng tiền vào, trạng thái tĩnh
//     [OrderStatus.ConfirmationPending]: "warning", // Cam: Cần Manager bấm xác nhận ngay
//     [OrderStatus.Confirmed]: "primary", // Xanh dương: Đã tiếp nhận đơn
//     [OrderStatus.Preparing]: "secondary", // Tím: Đang chế biến (phân biệt với các bước khác)
//     [OrderStatus.ReadyForPickup]: "success", // Xanh lá: Xong rồi, gọi khách thôi!
//     [OrderStatus.Delivering]: "primary", // Xanh dương: Đang trên đường đi
//     [OrderStatus.Completed]: "success", // Xanh lá: Kết thúc tốt đẹp
//     [OrderStatus.Cancelled]: "danger" // Đỏ: Có vấn đề hoặc bị hủy
// }

export const OrderStatusColors: Record<OrderStatus, ChipProps["color"]> = {
    [OrderStatus.PaymentPending]: "default",
    [OrderStatus.Preparing]: "secondary",
    [OrderStatus.Ready]: "primary",
    [OrderStatus.Completed]: "success",
    [OrderStatus.Cancel]: "danger"
}
