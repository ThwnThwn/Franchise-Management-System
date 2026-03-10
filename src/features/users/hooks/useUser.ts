import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { userApi } from "../services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError, PageResult } from "@/models/common/respone"
import { UserFilterParams } from "../models/request"
import { PaginationParams } from "@/models/common/request"
import { User } from "@/models/user"
import { useMemo } from "react"

export const useGetAllUsers = ({
    params,
    pagination = {},
    enabled = true
}: {
    params: UserFilterParams
    pagination: PaginationParams
    enabled?: boolean
}) => {
    const queryClient = useQueryClient()

    return useQuery({
        queryKey: [QUERY_KEYS.USERS, params, pagination],
        queryFn: () => userApi.getAll({ query: params, pagination }),
        initialData: () => {
            return queryClient.getQueryData<PageResult<User>>([
                QUERY_KEYS.USERS,
                params,
                pagination
            ])
        },
        enabled
    })
}

/**
 * @template T - Kiểu dữ liệu của item trong mảng (Transaction, Withdrawal, Order...)
 * @param items - Mảng dữ liệu cần bơm info User
 * @param idSelector - Hàm để trích xuất ID từ item (VD: item => item.userId)
 */
export const useHydrateUsers = <T>(
    items: T[] | undefined,
    idSelector: (item: T) => string | undefined | null
) => {
    const userIds = useMemo(() => {
        // Lấy ID dựa trên selector, lọc bỏ null/undefined và lọc trùng
        const ids = (items?.map(idSelector).filter(Boolean) as string[]) || []
        return [...new Set(ids)]
    }, [items, idSelector])

    const { data: usersRes, isLoading } = useGetAllUsers({
        params: { ids: userIds },
        pagination: { page: 1, pageSize: userIds.length || 1 },
        enabled: userIds.length > 0
    })

    const getUserInfo = (userId: string) => usersRes?.items.find((u) => u.id === userId)

    return { getUserInfo, isLoadingUsers: isLoading }
}

export const useGetUserById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    return useQuery({
        queryKey: [QUERY_KEYS.USERS, id],
        queryFn: async () => {
            const data = await userApi.getById(id)
            return data
        },
        enabled: enabled
    })
}

export const useCreateUser = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: userApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.USERS],
                exact: false
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Tạo mới thành công",
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

export const useUpdateUser = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: userApi.update,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.USERS],
                exact: false
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thành công",
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

export const useUpdateUserRole = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: userApi.updateRole,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: [QUERY_KEYS.USERS]
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
                description: "Cập nhật thành công",
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
