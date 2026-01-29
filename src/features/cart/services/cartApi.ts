import { requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { AddToCartRequest, DeleteListCartItemRequest, UpdateCartItemRequest } from "../models/schema/request"
import { GetCartResponse } from "../models/schema/response"

export const cartApi = {
    getCart: () =>
        requestWrapper<GetCartResponse>(async () => {
            const res = await axiosInstance.get("/carts")
            return res.data
        }),

    addToCart: (data: AddToCartRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/carts", data)
        }),

    updateCartItem: (data: UpdateCartItemRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.put("/carts", data)
        }),

    deleteListCartItem: (data: DeleteListCartItemRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete("/carts", { data })
        }),

    clearCart: () =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete("/carts/clear")
        })
}
