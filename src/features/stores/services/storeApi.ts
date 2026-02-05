import { buildQueryParams, requestWrapper } from "@/utils/helpers/axiosHelper"
import axiosInstance from "@/utils/axios"
import { FindNearbyStoresParams, GetStoresParams } from "../models/schema/request"
import { StoreDetailDto } from "../models/schema/response"
import { PaginationParams } from "@/models/common/request"
import { PageResult } from "@/models/common/respone"

export const storeApi = {
    getAll: ({ pagination, query }: { pagination: PaginationParams; query: GetStoresParams }) =>
        requestWrapper<PageResult<StoreDetailDto>>(async () => {
            const params = {
                ...buildQueryParams(query),
                ...buildQueryParams(pagination)
            }
            const res = await axiosInstance.get("/stores", { params })
            return res.data
        }),

    getDetail: (id: string) =>
        requestWrapper<StoreDetailDto>(async () => {
            const res = await axiosInstance.get(`/stores/${id}`)
            return res.data
        }),

    getNearby: (query: FindNearbyStoresParams) =>
        requestWrapper<PageResult<StoreDetailDto>>(async () => {
            const params = buildQueryParams(query)
            const res = await axiosInstance.get("/stores/nearby", { params })
            return res.data
        })
}
