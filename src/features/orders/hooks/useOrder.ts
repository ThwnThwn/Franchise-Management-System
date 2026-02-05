import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { ordersApi } from "@/features/orders/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { ORDER_TEXT } from "@/features/orders/constants"
import { BackendError } from "@/models/common/respone"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { OrderReply } from "../models/response"

export const useGetMyOrders = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.ORDERS, QUERY_KEYS.ME],
        queryFn: ordersApi.getMyOrders
    })
}

export const useGetOrderById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.ORDERS, id],
        queryFn: async () => {
            const data = await ordersApi.getById({ id })
            return data
        },
        enabled: enabled
    })
}

export const useCreateOrder = ({ onSuccess }: { onSuccess?: (res: OrderReply) => void } = {}) => {
    const queryClient = useQueryClient()
    const roleSlug = useTokenStore((s) => s.roleSlug)
    const isCustomer = roleSlug === RoleSlug.Customer
    const queryKey = isCustomer ? [QUERY_KEYS.ORDERS, QUERY_KEYS.ME] : [QUERY_KEYS.ORDERS]

    return useMutation({
        mutationFn: ordersApi.createOrder,
        onSuccess: (res) => {
            onSuccess?.(res)
            if (res.checkStockResponse?.isEnough === true) {
                queryClient.invalidateQueries({
                    queryKey: queryKey,
                    exact: false
                })
                addToast({
                    title: STRINGS.COMMON.SUCCESS,
                    description: ORDER_TEXT.labels.CREATED_SUCCESS,
                    color: "success"
                })
            }
        },
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}

export const usePayment = () => {
    return useMutation({
        mutationFn: ordersApi.payment,
        onError: (error: BackendError) => {
            addToast({
                title: error.title || STRINGS.COMMON.ERROR,
                description: error.detail,
                color: "danger"
            })
        }
    })
}
