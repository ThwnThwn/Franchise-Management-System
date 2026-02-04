import { QUERY_KEYS } from "@/constants/queryKeys"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { permissionApi } from "../services"
import { UpdatePermissionInfoRequest } from "../models/request"
import { addToast } from "@heroui/react"
import { STRINGS } from "@/constants/strings"
import { BackendError } from "@/models/common/respone"

export const useGetAllPermissions = ({ enabled = true }: { enabled?: boolean }) => {
    const key = QUERY_KEYS.PERMISSIONS
    return useQuery({
        queryKey: key,
        queryFn: permissionApi.getAll,
        enabled: enabled
    })
}
export const useUpdatePermission = ({ id, onSuccess }: { id: string; onSuccess?: () => void }) => {
    const queryClient = useQueryClient()
    const key = QUERY_KEYS.PERMISSIONS
    return useMutation({
        mutationFn: async (req: UpdatePermissionInfoRequest) => {
            await permissionApi.update(id, req)
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
