import {
    OrderReply,
    Order,
    OrderSummary,
    GetRevenueReportReply,
    GetOrderVolumeReply
} from "@/features/orders/models/response"
import axiosInstance from "@/utils/axios"
import {
    CreateOrderRequest,
    CreatePaymentRequest,
    GetOrdersQuery,
    GetRevenueReportQuery,
    GetOrderVolumeQuery,
    UpdateOrderItem,
    AssignStaffToDeliveryRequest
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
    },

    assignStaffToDelivery: async ({
        id,
        req
    }: {
        id: string
        req: AssignStaffToDeliveryRequest
    }): Promise<void> => {
        await axiosInstance.post(`/orders/${id}/delivery/assign`, req)
    },

    selfAssignStaffToDelivery: async ({ id }: { id: string }): Promise<void> => {
        await axiosInstance.post(`/orders/${id}/delivery/me/assign`)
    },

    // ── Analytics ──────────────────────────────────────────────────────────────

    /** GET /orders/reports/revenue - Doanh thu theo sản phẩm */
    getRevenueReport: async (query: GetRevenueReportQuery): Promise<GetRevenueReportReply> => {
        const res = await axiosInstance.get("/orders/reports/revenue", {
            params: buildQueryParams(query)
        })
        return res.data
    },

    /** GET /orders/reports/volume - Số đơn theo ngày */
    getOrderVolume: async (query: GetOrderVolumeQuery): Promise<GetOrderVolumeReply> => {
        const res = await axiosInstance.get("/orders/reports/volume", {
            params: buildQueryParams(query)
        })
        return res.data
    },

    /** GET /orders/reports/revenue/export - Tải về CSV doanh thu */
    exportRevenueReport: async (query: GetRevenueReportQuery): Promise<void> => {
        const res = await axiosInstance.get("/orders/reports/revenue/export", {
            params: buildQueryParams(query),
            responseType: "blob"
        })
        const cd = res.headers["content-disposition"] as string
        const filename = cd?.match(/filename=([^;]+)/)?.[1] ?? "revenue-report.csv"
        const url = URL.createObjectURL(new Blob([res.data]))
        const a = document.createElement("a")
        a.href = url
        a.download = filename
        a.click()
        URL.revokeObjectURL(url)
    },

    /** GET /orders/reports/volume/export - Tải về CSV khối lượng đơn */
    exportOrderVolume: async (query: GetOrderVolumeQuery): Promise<void> => {
        const res = await axiosInstance.get("/orders/reports/volume/export", {
            params: buildQueryParams(query),
            responseType: "blob"
        })
        const cd = res.headers["content-disposition"] as string
        const filename = cd?.match(/filename=([^;]+)/)?.[1] ?? "order-volume.csv"
        const url = URL.createObjectURL(new Blob([res.data]))
        const a = document.createElement("a")
        a.href = url
        a.download = filename
        a.click()
        URL.revokeObjectURL(url)
    }
}
