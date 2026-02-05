import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { OrderReply, Order, OrderSummary } from "@/features/orders/models/response"
import axiosInstance from "@/utils/axios"
import { CreateOrderRequest } from "@/features/orders/models/request"

export const ordersApi = {
    getById: ({ id }: { id: string }) =>
        requestWrapper<Order>(async () => {
            const res = await axiosInstance.get(`/orders/${id}`)
            return res.data
        }),

    getMyOrders: () =>
        requestWrapper<OrderSummary[]>(async () => {
            const res = await axiosInstance.get("/orders/me")
            return res.data
        }),

    createOrder: (req: CreateOrderRequest) =>
        requestWrapper<OrderReply>(async () => {
            const res = await axiosInstance.post("/orders", req)
            return res.data
        }),

    payment: (id: string) =>
        requestWrapper<{ paymentUrl: string }>(async () => {
            const res = await axiosInstance.post(`/orders/${id}/payment`)
            return res.data
        })
}
