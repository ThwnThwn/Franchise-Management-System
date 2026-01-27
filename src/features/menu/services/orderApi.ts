import axiosInstance from "@/utils/axios"
import { requestWrapper } from "@/utils/helpers/axiosHelper"

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
}

export const orderApi = {
    create: (payload: CreateOrderPayload) =>
        requestWrapper<CreateOrderResponse>(async () => {
            const res = await axiosInstance.post("/orders", payload)
            return res.data
        })
}

