import { QUERY_KEYS } from "@/constants/queryKeys"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import { useQuery } from "@tanstack/react-query"
import { storeApi } from "../services"

export const useGetStores = (params: GetStoresParams) => {
    const key = [...QUERY_KEYS.STORES, params]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getAll(params)
        }
    })
}

export const useGetStoreById = (id: string) => {
    const key = [...QUERY_KEYS.STORES, id]
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
    const key = [...QUERY_KEYS.STORES, params]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getNearby(params)
        },
        enabled: enabled
    })
}
