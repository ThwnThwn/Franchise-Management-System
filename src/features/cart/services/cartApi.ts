import { requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import {
    AddToCartRequest,
    DeleteListCartItemRequest,
    UpdateCartItemRequest
} from "../models/schema/request"
import { GetCartResponse } from "../models/schema/response"

export const cartApi = {
    getCart: () =>
        requestWrapper<GetCartResponse>(async () => {
            const res = await axiosInstance.get("/me/cart")
            return res.data
        }),

    addToCart: (data: AddToCartRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.post("/me/cart/items", data)
        }),

    updateCartItem: (data: UpdateCartItemRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.put("/me/cart/items", data)
        }),

    deleteListCartItem: (data: DeleteListCartItemRequest) =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete("/me/cart/items", { data })
        }),

    clearCart: () =>
        requestWrapper<void>(async () => {
            await axiosInstance.delete("/me/cart")
        })
}
