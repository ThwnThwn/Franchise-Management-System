import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { inventoryApi } from "../services/inventoryApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { GetAllStockParams } from "../models/request"

export const useGetAllStocks = ({
    params,
    enabled = true
}: {
    params: GetAllStockParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.INVENTORY, params],
        queryFn: () => inventoryApi.getAllStocks(params),
        enabled
    })
}

export const useAdjustStock = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: inventoryApi.adjustStock,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.INVENTORY]
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Điều chỉnh tồn kho thành công",
                color: "success"
            })
            onSuccess?.()
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
