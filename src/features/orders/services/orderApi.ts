import { OrderReply, Order, OrderSummary } from "@/features/orders/models/response"
import axiosInstance from "@/utils/axios"
import {
    CreateOrderRequest,
    CreatePaymentRequest,
    GetOrdersQuery,
    UpdateOrderItem
} from "@/features/orders/models/request"
import { OrderStatus } from "../constants"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"
import { buildQueryParams } from "@/utils/helpers/axiosHelper"

/**
 * ORDERS API
 * Quản lý đơn hàng và thanh toán.
 */
export const ordersApi = {
    getAll: async ({
        query,
        pagination
    }: {
        query: GetOrdersQuery
        pagination: PaginationParams
    }): Promise<PageResult<OrderSummary>> => {
        const params = {
            ...buildQueryParams(query),
            ...buildQueryParams(pagination)
        }
        const res = await axiosInstance.get("/orders", { params })
        return res.data
    },

    // Lấy chi tiết đơn hàng
    getById: async ({ id }: { id: string }): Promise<Order> => {
        const res = await axiosInstance.get(`/orders/${id}`)
        return res.data
    },

    // Lấy danh sách đơn hàng của tôi (Khách hàng/Nhân viên)
    getMyOrders: async (): Promise<OrderSummary[]> => {
        const res = await axiosInstance.get("/me/orders")
        return res.data
    },

    // Tạo đơn hàng mới
    createOrder: async (req: CreateOrderRequest): Promise<OrderReply> => {
        const res = await axiosInstance.post("/orders", req)
        return res.data
    },

    // Cập nhật các món ăn trong đơn hàng
    updateOrder: async ({
        id,
        req
    }: {
        id: string
        req: UpdateOrderItem[]
    }): Promise<OrderReply> => {
        const res = await axiosInstance.put(`/orders/${id}`, req)
        return res.data
    },

    // Thay đổi trạng thái đơn hàng (VD: Đang chuẩn bị -> Đã giao)
    updateOrderStatus: async ({
        id,
        status
    }: {
        id: string
        status: OrderStatus
    }): Promise<void> => {
        await axiosInstance.patch(`/orders/${id}/status`, { status })
    },

    // Xử lý thanh toán và lấy URL redirect
    payment: async ({
        id,
        req
    }: {
        id: string
        req: CreatePaymentRequest
    }): Promise<{ paymentUrl: string }> => {
        const res = await axiosInstance.post(`/orders/${id}/payment`, {
            ...req,
            paymentMethod: Number(req.paymentMethod)
        })
        return res.data
    }
}
