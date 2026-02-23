import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { ordersApi } from "@/features/orders/services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { ORDER_TEXT, OrderStatus } from "@/features/orders/constants"
import { BackendError, PageResult } from "@/models/common/respone"
import { useTokenStore } from "@/hooks"
import { RoleSlug } from "@/constants/enums"
import { OrderReply, OrderSummary } from "@/features/orders/models/response"
import { CreatePaymentRequest, GetOrdersQuery, UpdateOrderItem } from "../models/request"
import { PaginationParams } from "@/models/common/request"

export const useGetMyOrders = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.ORDERS, QUERY_KEYS.ME],
        queryFn: ordersApi.getMyOrders
    })
}

export const useGetOrders = ({
    params,
    pagination = {},
    enabled = true
}: {
    params: GetOrdersQuery
    pagination: PaginationParams
    enabled?: boolean
}) => {
    const queryClient = useQueryClient()

    return useQuery({
        queryKey: [QUERY_KEYS.PRODUCTS, params],
        queryFn: () => ordersApi.getAll({ query: params, pagination }),
        initialData: () => {
            return queryClient.getQueryData<PageResult<OrderSummary>>([
                QUERY_KEYS.USERS,
                params,
                pagination
            ])
        },
        enabled
    })
}

export const useGetOrderById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.ORDERS, id],
        queryFn: async () => {
            const data = await ordersApi.getById({ id })
            console.log(data)

            return data
        },
        enabled: enabled
    })
}

export const useCreateOrder = ({ onSuccess }: { onSuccess?: (res: OrderReply) => void } = {}) => {
    const queryClient = useQueryClient()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
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

export const useUpdateOrder = ({
    id,
    onSuccess
}: {
    id: string
    onSuccess?: (res: OrderReply) => void
}) => {
    const queryClient = useQueryClient()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const queryKey = isCustomer ? [QUERY_KEYS.ORDERS, QUERY_KEYS.ME] : [QUERY_KEYS.ORDERS]

    return useMutation({
        mutationFn: async (req: UpdateOrderItem[]) => {
            return await ordersApi.updateOrder({
                id: id,
                req: req
            })
        },
        onSuccess: (res) => {
            onSuccess?.(res)
            if (res.checkStockResponse?.isEnough === true) {
                queryClient.invalidateQueries({
                    queryKey: queryKey,
                    exact: false
                })
                queryClient.invalidateQueries({
                    queryKey: [QUERY_KEYS.ORDERS, id]
                })
                addToast({
                    title: STRINGS.COMMON.SUCCESS,
                    description: ORDER_TEXT.labels.UPDATE_SUCCESS,
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

export const useUpdateOrderStatus = ({ id, onSuccess }: { id: string; onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const queryKey = isCustomer ? [QUERY_KEYS.ORDERS, QUERY_KEYS.ME] : [QUERY_KEYS.ORDERS]

    return useMutation({
        mutationFn: async (status: OrderStatus) => {
            return await ordersApi.updateOrderStatus({
                id: id,
                status: status
            })
        },
        onSuccess: () => {
            onSuccess?.()
            queryClient.invalidateQueries({
                queryKey: queryKey,
                exact: false
            })
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ORDERS, id]
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: ORDER_TEXT.labels.UPDATE_SUCCESS,
                color: "success"
            })
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

export const usePayment = ({
    id,
    onSuccess
}: {
    id: string
    onSuccess?: ({ paymentUrl }: { paymentUrl: string }) => void
}) => {
    const queryClient = useQueryClient()
    const isCustomer = useTokenStore((s) => s.roleSlug === RoleSlug.Customer)
    const queryKey = isCustomer ? [QUERY_KEYS.ORDERS, QUERY_KEYS.ME] : [QUERY_KEYS.ORDERS]

    return useMutation({
        mutationFn: async (req: CreatePaymentRequest) => {
            return await ordersApi.payment({
                id: id,
                req: req
            })
        },
        onSuccess: (res) => {
            onSuccess?.(res)
            queryClient.invalidateQueries({
                queryKey: queryKey,
                exact: false
            })
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.ORDERS, id]
            })
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
