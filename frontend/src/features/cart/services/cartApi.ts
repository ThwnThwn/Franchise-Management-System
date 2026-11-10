import axiosInstance from "@/utils/axios"
import {
    AddToCartRequest,
    DeleteListCartItemRequest,
    UpdateCartItemRequest
} from "../models/schema/request"
import { GetCartResponse } from "../models/schema/response"

/**
 * CART API
 */
export const cartApi = {
    // Lấy thông tin giỏ hàng
    getCart: async (): Promise<GetCartResponse> => {
        const res = await axiosInstance.get("/me/cart")
        return res.data
    },

    // Thêm sản phẩm vào giỏ
    addToCart: async (data: AddToCartRequest): Promise<void> => {
        await axiosInstance.post("/me/cart/items", data)
    },

    // Cập nhật số lượng/thuộc tính item trong giỏ
    updateCartItem: async (data: UpdateCartItemRequest): Promise<void> => {
        await axiosInstance.put("/me/cart/items", data)
    },

    // Xóa danh sách sản phẩm đã chọn khỏi giỏ
    deleteListCartItem: async (data: DeleteListCartItemRequest): Promise<void> => {
        await axiosInstance.delete("/me/cart/items", { data })
    },

    // Xóa sạch giỏ hàng
    clearCart: async (): Promise<void> => {
        await axiosInstance.delete("/me/cart")
    }
}
