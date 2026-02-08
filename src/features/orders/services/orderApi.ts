import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { OrderReply, Order, OrderSummary } from "@/features/orders/models/response"
import axiosInstance from "@/utils/axios"
import {
    CreateOrderRequest,
    CreatePaymentRequest,
    UpdateOrderItem
} from "@/features/orders/models/request"
import { OrderStatus } from "../constants"

export const ordersApi = {
    getById: ({ id }: { id: string }) =>
        requestWrapper<Order>(async () => {
            const res = await axiosInstance.get(`/orders/${id}`)
            return res.data
        }),

    getMyOrders: () =>
        requestWrapper<OrderSummary[]>(async () => {
            const res = await axiosInstance.get("/me/orders")
            return res.data
        }),

    createOrder: (req: CreateOrderRequest) =>
        requestWrapper<OrderReply>(async () => {
            const res = await axiosInstance.post("/orders", req)
            return res.data
        }),

    updateOrder: ({ id, req }: { id: string; req: UpdateOrderItem[] }) =>
        requestWrapper<OrderReply>(async () => {
            const res = await axiosInstance.put(`/orders/${id}`, req)
            return res.data
        }),

    updateOrderStatus: ({ id, status }: { id: string; status: OrderStatus }) =>
        requestWrapper<void>(async () => {
            const res = await axiosInstance.patch(`/orders/${id}/status`, { status })
            return res.data
        }),

    payment: ({ id, req }: { id: string; req: CreatePaymentRequest }) =>
        requestWrapper<{ paymentUrl: string }>(async () => {
            const res = await axiosInstance.post(`/orders/${id}/payment`, {
                ...req,
                paymentMethod: Number(req.paymentMethod)
            })
            return res.data
        })
}
