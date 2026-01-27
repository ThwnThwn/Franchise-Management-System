import axiosInstance from "@/utils/axios"
import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import { GetMyOrdersResponse, Order } from "../models/schema/order"

export type CreateOrderItemPayload = {
    sku: string
    quantity: number
}

export type CreateOrderPayload = {
    items: CreateOrderItemPayload[]
    tax?: number
    paymentMethod?: number
    notes?: string
}

export type CreateOrderResponse = {
    orderId: string
    orderCode: string
    totalAmount: number
    customerId?: string
}

export type GetMyOrdersParams = {
    page?: number
    pageSize?: number
}

export const orderApi = {
    create: (payload: CreateOrderPayload) =>
        requestWrapper<CreateOrderResponse>(async () => {
            const res = await axiosInstance.post("/orders/", payload)
            return res.data
        }),

    getMyOrders: (params?: GetMyOrdersParams) =>
        requestWrapper<GetMyOrdersResponse>(async () => {
            const queryParams = buildQueryParams(params ?? {})
            const res = await axiosInstance.get("/orders/me", { params: queryParams })
            return res.data
        }),

    getOrderById: (id: string) =>
        requestWrapper<Order>(async () => {
            const res = await axiosInstance.get(`/orders/${id}`)
            return res.data
        })
}

