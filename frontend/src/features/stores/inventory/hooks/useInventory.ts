import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { inventoryApi } from "../services/inventoryApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import {
    GetAllStockParams,
    GetIngredientsParams,
    CreateIngredientRequest,
    UpdateIngredientRequest,
    AddIngredientStockItem,
    CreateIngredientCategoryRequest,
    UpdateIngredientCategoryRequest
} from "../models/request"

// =============================================
// STOCK HOOKS
// =============================================

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

export const useGetStockByStoreId = ({
    storeId,
    params,
    enabled = true
}: {
    storeId: string
    params: GetAllStockParams
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.INVENTORY, storeId, params],
        queryFn: () => inventoryApi.getStockByStoreId(storeId, params),
        enabled: enabled && !!storeId
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

// =============================================
// INGREDIENT HOOKS
// =============================================

export const useGetIngredients = ({
    params,
    enabled = true
}: {
    params?: GetIngredientsParams
    enabled?: boolean
} = {}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.INGREDIENTS, params],
        queryFn: () => inventoryApi.getIngredients(params),
        enabled
    })
}

export const useGetIngredientCategories = ({ enabled = true }: { enabled?: boolean } = {}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.INGREDIENT_CATEGORIES],
        queryFn: () => inventoryApi.getIngredientCategories(),
        enabled
    })
}

export const useCreateIngredientCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (req: CreateIngredientCategoryRequest) =>
            inventoryApi.createIngredientCategory(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENT_CATEGORIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo danh mục nguyên liệu thành công",
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

export const useUpdateIngredientCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateIngredientCategoryRequest }) =>
            inventoryApi.updateIngredientCategory(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENT_CATEGORIES] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật danh mục nguyên liệu thành công",
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

export const useDeleteIngredientCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => inventoryApi.deleteIngredientCategory(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENT_CATEGORIES] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Xóa danh mục nguyên liệu thành công",
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

export const useCreateIngredient = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (req: CreateIngredientRequest) => inventoryApi.createIngredient(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo nguyên liệu thành công",
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

export const useUpdateIngredient = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateIngredientRequest }) =>
            inventoryApi.updateIngredient(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật nguyên liệu thành công",
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

export const useDeleteIngredient = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: string) => inventoryApi.deleteIngredient(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Xóa nguyên liệu thành công",
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

/**
 * Hook nhập kho nguyên liệu
 * Gọi PUT /stores/:storeId/stock với body là mảng IngredientStockItem[]
 */
export const useAddIngredientStock = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ storeId, items }: { storeId: string; items: AddIngredientStockItem[] }) =>
            inventoryApi.increaseStoreStock(storeId, items),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INVENTORY] })
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.INGREDIENTS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Ingredient stock successfully added.",
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
