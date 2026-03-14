import { ChipProps } from "@heroui/react"
import { OrderSaleItemType, OrderStatus } from "./enum"
import { Coins, LucideProps, Tag, Ticket, Wallet } from "lucide-react"
import React from "react"

export const OrderStatusColors: Record<OrderStatus, ChipProps["color"]> = {
    [OrderStatus.PaymentPending]: "default", // Xám: Chưa có dòng tiền vào, trạng thái tĩnh
    [OrderStatus.ConfirmationPending]: "warning", // Cam: Cần Manager bấm xác nhận ngay
    [OrderStatus.Confirmed]: "primary", // Xanh dương: Đã tiếp nhận đơn
    [OrderStatus.Preparing]: "secondary", // Tím: Đang chế biến (phân biệt với các bước khác)
    [OrderStatus.ReadyForPickup]: "success", // Xanh lá: Xong rồi, gọi khách thôi!
    [OrderStatus.Delivering]: "primary", // Xanh dương: Đang trên đường đi
    [OrderStatus.Completed]: "success", // Xanh lá: Kết thúc tốt đẹp
    [OrderStatus.Cancelled]: "danger" // Đỏ: Có vấn đề hoặc bị hủy
}

export const OrderSaleItemTypeColors: Record<OrderSaleItemType, ChipProps["color"]> = {
    [OrderSaleItemType.VOUCHER]: "warning", // Vàng cam
    [OrderSaleItemType.WALLET]: "success", // Xanh lá (tiền bạc)
    [OrderSaleItemType.POINT]: "primary", // Màu chủ đạo của app (điểm)
    [OrderSaleItemType.PROMOTION]: "danger" // Đỏ (hot deal/sale)
}

export const OrderSaleItemTypeIcons: Record<
    OrderSaleItemType,
    React.ForwardRefExoticComponent<Omit<LucideProps, "ref"> & React.RefAttributes<SVGSVGElement>>
> = {
    [OrderSaleItemType.VOUCHER]: Ticket,
    [OrderSaleItemType.WALLET]: Wallet,
    [OrderSaleItemType.POINT]: Coins,
    [OrderSaleItemType.PROMOTION]: Tag
}
