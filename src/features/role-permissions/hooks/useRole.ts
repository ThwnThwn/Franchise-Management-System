import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { roleApi } from "../services"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"
import { UpdateRolePermissionsRequest, UpdateRoleRequest } from "../models/request"

export const useGetAllRoles = ({ enabled = true }: { enabled?: boolean }) => {
    const key = QUERY_KEYS.ROLES
    return useQuery({
        queryKey: key,
        queryFn: roleApi.getAll,
        enabled: enabled
    })
}

export const useCreateRole = ({ onSuccess }: { onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const key = QUERY_KEYS.ROLES
    return useMutation({
        mutationFn: roleApi.create,
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: key
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
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

export const useUpdateRole = ({ id, onSuccess }: { id: string; onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const key = QUERY_KEYS.ROLES
    return useMutation({
        mutationFn: async (req: UpdateRoleRequest) => {
            await roleApi.update(id, req)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: key
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
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

export const useGetPermissionsByRoleId = ({
    id,
    enabled = true
}: {
    id: string
    enabled?: boolean
}) => {
    const key = [...QUERY_KEYS.ROLES, id, ...QUERY_KEYS.PERMISSIONS]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await roleApi.getPermissionsByRoleId(id)
        },
        enabled: enabled
    })
}

export const useUpdateRolePermissions = ({
    id,
    onSuccess
}: {
    id: string
    onSuccess?: () => void
}) => {
    const queryClient = useQueryClient()
    const key = [...QUERY_KEYS.ROLES, id, ...QUERY_KEYS.PERMISSIONS]

    return useMutation({
        mutationFn: async (req: UpdateRolePermissionsRequest) => {
            return await roleApi.updateRolePermissions(id, req)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: key
            })
            addToast({
                title: STRINGS.COMMON.SUCCESS,
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
