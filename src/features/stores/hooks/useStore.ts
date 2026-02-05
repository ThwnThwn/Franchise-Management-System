import { QUERY_KEYS } from "@/constants/queryKeys"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import { useQuery, useQueryClient } from "@tanstack/react-query"
import { storeApi } from "../services"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"
import { StoreDetailDto } from "../models/schema/response"

export const useGetStores = ({
    pagination,
    params,
    enabled = true
}: {
    pagination: PaginationParams
    params: GetStoresParams
    enabled?: boolean
}) => {
    const key = [QUERY_KEYS.STORES, params, pagination]
    const queryClient = useQueryClient()
    return useQuery({
        queryKey: key,
        queryFn: () => storeApi.getAll({ query: params, pagination }),
        initialData: () => {
            return queryClient.getQueryData<PageResult<StoreDetailDto>>(key)
        },
        enabled: enabled
    })
}

export const useGetStoreById = (id: string) => {
    const key = [QUERY_KEYS.STORES, id]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getDetail(id)
        }
    })
}

export const useGetNearbyStores = ({
    params,
    enabled
}: {
    params: FindNearbyStoresParams
    enabled: boolean
}) => {
    const key = [QUERY_KEYS.STORES, params]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getNearby(params)
        },
        enabled: enabled
    })
}
