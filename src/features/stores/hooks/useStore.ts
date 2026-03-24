import { QUERY_KEYS } from "@/constants/queryKeys"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import { useInfiniteQuery, useQuery, useQueryClient } from "@tanstack/react-query"
import { storeApi } from "../services"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"
import { StoreDetailDto } from "../models/schema/response"

export const useGetInfiniteStores = ({
    params,
    pageSize = 4,
    enabled = true
}: {
    params: GetStoresParams
    pageSize?: number
    enabled?: boolean
}) => {
    return useInfiniteQuery({
        queryKey: [QUERY_KEYS.STORES, params],
        queryFn: async ({ pageParam = 1 }) => {
            const response = await storeApi.getAll({
                pagination: {
                    page: pageParam,
                    pageSize: pageSize
                },
                query: params
            })
            return response
        },
        initialPageParam: 1,
        getNextPageParam: (lastPage, allPages) => {
            // allPages.length chính là số trang hiện tại đã load thành công
            const currentPage = allPages?.length ?? 0

            // Nếu trang hiện tại vẫn nhỏ hơn tổng số trang backend báo về
            return currentPage < lastPage.totalPages ? currentPage + 1 : undefined
        },
        enabled: enabled
    })
}

export const useGetStores = ({
    pagination,
    params,
    enabled = true
}: {
    pagination: PaginationParams
    params: GetStoresParams
    enabled?: boolean
}) => {
    const key = [QUERY_KEYS.STORES, params, pagination, "all"]
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

export const useGetStoreById = ({ id, enabled = true }: { id: string; enabled?: boolean }) => {
    const key = [QUERY_KEYS.STORES, id]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getDetail(id)
        },
        enabled: enabled
    })
}

export const useGetNearbyStores = ({
    params,
    enabled
}: {
    params: FindNearbyStoresParams
    enabled: boolean
}) => {
    const key = [QUERY_KEYS.STORES, params, "nearby"]
    return useQuery({
        queryKey: key,
        queryFn: async () => {
            return await storeApi.getNearby(params)
        },
        enabled: enabled
    })
}

export const useGetActiveStaffs = ({
    storeId,
    enabled = true
}: {
    storeId?: string
    enabled?: boolean
}) => {
    return useQuery({
        queryKey: [QUERY_KEYS.STORES, QUERY_KEYS.STORE_ACTIVE_STAFFS, storeId],
        queryFn: async () => {
            if (!storeId) return []
            return await storeApi.getActiveStaffs(storeId)
        },
        enabled: enabled && !!storeId
    })
}
