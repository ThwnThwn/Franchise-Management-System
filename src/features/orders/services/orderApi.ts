import { requestWrapper } from "@/utils/helpers/axiosHelper"
import { Order } from "@/features/orders/models/response"
import axiosInstance from "@/utils/axios"

export const ordersApi = {
    getMyOrders: () =>
        requestWrapper<Order[]>(async () => {
            const res = await axiosInstance.get("/orders/me")
            return res.data
        })
}
