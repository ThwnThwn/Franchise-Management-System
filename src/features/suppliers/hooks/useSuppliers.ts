import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { supplierApi } from "../services/supplierApi"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { CreateSupplierRequest, GetSuppliersParams, UpdateSupplierRequest } from "../models/request"

export const useGetSuppliers = ({
    params,
    enabled = true
}: {
    params?: GetSuppliersParams
    enabled?: boolean
} = {}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SUPPLIERS, params],
        queryFn: () => supplierApi.getSuppliers(params),
        enabled
    })
}

export const useGetSupplierById = (id: string, enabled = true) => {
    return useQuery({
        queryKey: [QUERY_KEYS.SUPPLIERS, id],
        queryFn: () => supplierApi.getSupplierById(id),
        enabled: enabled && !!id
    })
}

export const useCreateSupplier = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (req: CreateSupplierRequest) => supplierApi.createSupplier(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SUPPLIERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo nhà cung cấp thành công",
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

export const useUpdateSupplier = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: Omit<UpdateSupplierRequest, "id"> }) =>
            supplierApi.updateSupplier(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SUPPLIERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật nhà cung cấp thành công",
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

export const useDeleteSupplier = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: string) => supplierApi.deleteSupplier(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.SUPPLIERS] })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Xóa nhà cung cấp thành công",
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
