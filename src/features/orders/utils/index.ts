import { ORDER_STATUS_TRANSITION_POLICY, OrderStatus } from "@/features/orders/constants"

/**
 * Kiểm tra xem đơn hàng có nằm trong các trạng thái mà User (Staff/Admin)
 * CẦN phải thao tác thủ công để chuyển tiếp hay không.
 * * - PaymentPending (0): Chờ hệ thống tự động (Auto)
 * - Completed (6), Cancelled (7): Đã kết thúc (Terminal)
 */
export const isManualActionRequired = (status: OrderStatus): boolean => {
    // Chỉ những trạng thái từ 1 đến 5 mới cho phép Staff bấm nút chuyển tiếp
    return status >= OrderStatus.ConfirmationPending && status <= OrderStatus.Delivering
}

/**
 * Kiểm tra xem đơn hàng có đang ở trạng thái chờ Hệ thống tự động xử lý hay không.
 */
export const isAutoTransitionStatus = (status: OrderStatus): boolean => {
    return status === OrderStatus.PaymentPending
}

/**
 * Kiểm tra xem từ trạng thái hiện tại có thể thực hiện hành động "Hủy" (Cancel) hay không
 * dựa trên chính sách chuyển đổi trạng thái (Transition Policy).
 */
export const canCancelFrom = (status: OrderStatus): boolean => {
    const nextAllowedStatuses = ORDER_STATUS_TRANSITION_POLICY[status] || []

    // Check xem trong danh sách status tiếp theo có thằng Cancelled không
    return nextAllowedStatuses.includes(OrderStatus.Cancelled)
}
