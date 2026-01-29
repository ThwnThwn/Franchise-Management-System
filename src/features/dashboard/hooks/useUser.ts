import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { userApi } from "../services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

export const useGetAllUsers = () => {
    return useQuery({
        queryKey: QUERY_KEYS.USERS,
        queryFn: async () => {
            const data = await userApi.getAll()
            return data
        }
    })
}

export const useGetUserById = (id: string) => {
    return useQuery({
        queryKey: [...QUERY_KEYS.USERS, id],
        queryFn: async () => {
            const data = await userApi.getById(id)
            return data
        }
    })
}

export const useCreateUser = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: userApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.USERS
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

export const useUpdateUserRole = () => {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: userApi.updateRole,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: QUERY_KEYS.USERS
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
