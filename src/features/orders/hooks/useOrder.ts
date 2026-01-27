import { QUERY_KEYS } from "@/constants/queryKeys"
import { useQuery } from "@tanstack/react-query"
import { ordersApi } from "@/features/orders/services"

export const useGetMyOrders = () => {
    return useQuery({
        queryKey: [...QUERY_KEYS.ORDERS, ...QUERY_KEYS.ME],
        queryFn: async () => {
            const data = await ordersApi.getMyOrders()
            return data
        }
    })
}
