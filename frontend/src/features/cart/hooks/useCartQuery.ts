import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { cartApi } from "../services/cartApi"
import {
    AddToCartRequest,
    DeleteListCartItemRequest,
    UpdateCartItemRequest
} from "../models/schema/request"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"

const CART_QUERY_KEY = ["cart"]

export const useGetCart = ({ enabled }: { enabled: boolean }) => {
    return useQuery({
        queryKey: CART_QUERY_KEY,
        queryFn: cartApi.getCart,
        enabled: enabled
    })
}

export const useAddToCart = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (data: AddToCartRequest) => cartApi.addToCart(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
            addToast({
                title: "Thành công",
                description: "Đã thêm vào giỏ hàng",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: "Lỗi",
                description: error.detail || "Không thể thêm vào giỏ hàng",
                color: "danger"
            })
        }
    })
}

export const useUpdateCartItem = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (data: UpdateCartItemRequest) => cartApi.updateCartItem(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
        onError: (error: BackendError) => {
            addToast({
                title: "Lỗi",
                description: error.detail || "Không thể cập nhật giỏ hàng",
                color: "danger"
            })
        }
    })
}

export const useDeleteListCartItem = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (data: DeleteListCartItemRequest) => cartApi.deleteListCartItem(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
            addToast({
                title: "Thành công",
                description: "Đã xóa sản phẩm khỏi giỏ hàng",
                color: "success"
            })
        },
        onError: (error: BackendError) => {
            addToast({
                title: "Lỗi",
                description: error.detail || "Không thể xóa sản phẩm",
                color: "danger"
            })
        }
    })
}

export const useClearCart = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: cartApi.clearCart,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
        onError: (error: BackendError) => {
            console.error("Failed to clear cart", error)
        }
    })
}
