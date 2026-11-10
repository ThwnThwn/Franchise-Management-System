import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { QUERY_KEYS } from "@/constants/queryKeys"
import { categoryApi } from "../services"
import { addToast } from "@heroui/react"
import { BackendError } from "@/models/common/respone"
import { CreateCategoryRequest, UpdateCategoryRequest } from "../models/categories/request"

export const useGetAllCategories = () => {
    return useQuery({
        queryKey: [QUERY_KEYS.CATEGORIES],
        queryFn: () => categoryApi.getAll()
    })
}

export const useCreateCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (req: CreateCategoryRequest) => categoryApi.create(req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] })
            addToast({ title: "Thành công", description: "Đã tạo danh mục mới", color: "success" })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({ title: error.title || "Lỗi", description: error.detail, color: "danger" })
        }
    })
}

export const useUpdateCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: ({ id, req }: { id: string; req: UpdateCategoryRequest }) =>
            categoryApi.update(id, req),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] })
            addToast({ title: "Thành công", description: "Đã cập nhật danh mục", color: "success" })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({ title: error.title || "Lỗi", description: error.detail, color: "danger" })
        }
    })
}

export const useDeleteCategory = ({ onSuccess }: { onSuccess?: () => void } = {}) => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => categoryApi.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] })
            addToast({ title: "Thành công", description: "Đã xóa danh mục", color: "success" })
            onSuccess?.()
        },
        onError: (error: BackendError) => {
            addToast({ title: error.title || "Lỗi", description: error.detail, color: "danger" })
        }
    })
}
