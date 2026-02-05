import { QUERY_KEYS } from "@/constants/queryKeys"
import { useQueryClient } from "@tanstack/react-query"

export const useRemoveMeQuery = () => {
    const queryClient = useQueryClient()

    return () => queryClient.removeQueries({ queryKey: [QUERY_KEYS.ME] })
}

export const useInvalidateMeQuery = () => {
    const queryClient = useQueryClient()

    return () => queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.ME] })
}
