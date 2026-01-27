import { useMutation, useQueryClient } from "@tanstack/react-query"
import { orderApi, CreateOrderPayload, CreateOrderResponse } from "@/features/menu/services/orderApi"
import { useCart } from "./useCart"

export const useCreateOrder = () => {
    const queryClient = useQueryClient()
    const { clearCart } = useCart()

    return useMutation<CreateOrderResponse, Error, CreateOrderPayload>({
        mutationFn: orderApi.create,
        onSuccess: (data) => {
            // Clear cart after successful order
            clearCart()
            // Invalidate orders query to refetch
            queryClient.invalidateQueries({ queryKey: ["orders"] })
        }
    })
}
